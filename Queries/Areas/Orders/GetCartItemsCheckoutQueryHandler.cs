using _200SXContact.Data;
using _200SXContact.Models.DTOs.Areas.Orders;
using _200SXContact.Services;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetCartItemsCheckoutQueryHandler : IRequestHandler<GetCartItemsCheckoutQuery, List<CartItemDto?>?>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        public GetCartItemsCheckoutQueryHandler(ApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
        }
        public async Task<List<CartItemDto?>?> Handle(GetCartItemsCheckoutQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Checkout || Starting to get cart items for view in query", "Info", "");

            List<Models.Areas.Orders.CartItemModel>? cartItems = await _context.CartItems.Where(ci => ci.UserId == request.UserId).ToListAsync(cancellationToken);
            
            if (cartItems is null)
            {
                await _loggerService.LogAsync("Checkout || No cart items found for view in query", "Error", "");

                return null;
            }
            
            List<CartItemDto?> cartItemDtos = _mapper.Map<List<CartItemDto?>>(cartItems);

            await _loggerService.LogAsync("Checkout || Got cart items for view and returning", "Info", "");

            return cartItemDtos;
        }
    }
}
