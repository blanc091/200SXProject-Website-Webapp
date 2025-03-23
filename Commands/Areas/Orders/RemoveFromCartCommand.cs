﻿using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Orders;

namespace _200SXContact.Commands.Areas.Orders
{
    public class RemoveFromCartCommand : IRequest<bool>
    {
        public int ProductId { get; set; }
        public required string UserId { get; set; }
    }
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        public RemoveFromCartCommandHandler(IApplicationDbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
        }
        public async Task<bool> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Cart || Removing cart item", "Info", "");

            CartItem? cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.ProductId == request.ProductId && ci.UserId == request.UserId, cancellationToken);

            if (cartItem == null)
            {
                await _loggerService.LogAsync("Cart || Cart item is null when trying to remove it from the cart", "Error", "");

                return false;
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync("Cart || Cart item removed from cart", "Info", "");

            return true;
        }
    }
}
