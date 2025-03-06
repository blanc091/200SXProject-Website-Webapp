using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.Configs;
using _200SXContact.Services;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace _200SXContact.Commands.Areas.Orders
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, int>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly AdminSettings _adminSettings;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public PlaceOrderCommandHandler(ApplicationDbContext context, UserManager<User> userManager, IEmailService emailService, IOptions<AdminSettings> adminSettings, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _adminSettings = adminSettings.Value;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<int> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Finished validation, getting user", "Info", "");

            User? user = await _userManager.GetUserAsync(request.User);

            if (user == null)
            {
                await _loggerService.LogAsync("Orders || No user found when placing order", "Error", "");

                throw new UnauthorizedAccessException();
            }

            request.Model.UserId = user.Id;

            await _loggerService.LogAsync("Orders || Fetching cart items from DB", "Info", "");

            List<CartItem> cartItems = await _context.CartItems.Where(ci => ci.UserId == user.Id && ci.OrderId == null).ToListAsync(cancellationToken);

            if (!cartItems.Any())
            {
                await _loggerService.LogAsync("Orders || Cart is empty when trying to place order", "Error", "");

                throw new InvalidOperationException("Your cart is empty, please add items before checking out.");
            }

            request.Model.CartItemsJson = JsonSerializer.Serialize(cartItems);

            await _loggerService.LogAsync("Orders || Starting to save order", "Info", "");

            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    request.Model.OrderDate = DateTime.UtcNow;

                    OrderPlacement orderEntity = _mapper.Map<OrderPlacement>(request.Model);                    
                    await _context.Orders.AddAsync(orderEntity, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _loggerService.LogAsync("Orders || Order saved successfully", "Info", "");

                    foreach (CartItem? cartItem in cartItems)
                    {
                        cartItem.OrderId = orderEntity.Id;
                        _context.Entry(cartItem).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    await _loggerService.LogAsync("Orders || Cart items associated with order", "Info", "");

                    OrderTracking orderTracking = new OrderTracking
                    {
                        OrderId = orderEntity.Id,
                        Order = orderEntity,
                        Status = "Pending",
                        StatusUpdatedAt = DateTime.UtcNow,
                        Email = orderEntity.Email,
                        AddressLine = orderEntity.AddressLine1,
                        OrderNotes = orderEntity.OrderNotes,
                        CartItemsJson = orderEntity.CartItemsJson
                    };

                    await _context.OrderTrackings.AddAsync(orderTracking, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _loggerService.LogAsync("Orders || Order tracking saved successfully", "Info", "");
                    await _loggerService.LogAsync("Orders || Starting sending order emails", "Info", "");

                    await _emailService.SendOrderConfirmEmail(orderEntity.Email, orderEntity);
                    await _emailService.SendOrderConfirmEmail(_adminSettings.Email, orderEntity);

                    await _loggerService.LogAsync("Orders || Sent order emails", "Info", "");

                    _context.CartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _loggerService.LogAsync("Orders || Cart items removed from database after order placement", "Info", "");

                    await transaction.CommitAsync(cancellationToken);

                    await _loggerService.LogAsync("Orders || Committed transaction", "Info", "");

                    return orderEntity.Id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    await _loggerService.LogAsync($"Orders || Rolling back transaction due to exception: {ex.Message}", "Error", "");

                    throw;
                }
            }
        }
    }
}
