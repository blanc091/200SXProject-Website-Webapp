using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.Configs;
using _200SXContact.Models.DTOs.Areas.Orders;

namespace _200SXContact.Commands.Areas.Orders
{
    public class PlaceOrderCommand(OrderPlacementDto model, ClaimsPrincipal user) : IRequest<int>
    {
        public OrderPlacementDto Model { get; } = model;
        public ClaimsPrincipal User { get; } = user;
    }
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly AdminSettings _adminSettings;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        private readonly IClientTimeProvider _clientTimeProvider;
        public PlaceOrderCommandHandler(IClientTimeProvider clientTimeProvider, IApplicationDbContext context, UserManager<User> userManager, IEmailService emailService, IOptions<AdminSettings> adminSettings, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _adminSettings = adminSettings.Value;
            _loggerService = loggerService;
            _mapper = mapper;
            _clientTimeProvider = clientTimeProvider;
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
                    DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();

                    request.Model.OrderDate = clientTime;

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
                        Id = 0,
                        OrderId = orderEntity.Id,
                        Order = orderEntity,
                        Status = "Pending",
                        StatusUpdatedAt = clientTime,
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
