﻿using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Orders;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetCartItemsQuery(int orderId) : IRequest<List<CartItem>?>
    {
        public int OrderId { get; set; } = orderId;
    }
    public class GetCartItemsQueryHandler : IRequestHandler<GetCartItemsQuery, List<CartItem>?>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        public GetCartItemsQueryHandler(IApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
        }
        public async Task<List<CartItem>?> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Cart || Getting cart items admin", "Info", "");

            string? order = await _context.Orders.Where(o => o.Id == request.OrderId).Select(o => o.CartItemsJson).FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                await _loggerService.LogAsync("Cart || Order is null when getting cart items admin", "Error", "");

                return null;
            }

            List<CartItem> cartItems = _mapper.Map<List<CartItem>>(JsonSerializer.Deserialize<List<CartItem>>(order));

            await _loggerService.LogAsync("Cart || Got cart items admin", "Info", "");

            return cartItems ?? new List<CartItem>();
        }
    }
}