using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetCartItemsCountQuery : IRequest<int>
    {
        public required string UserId { get; set; }
    }
    public class GetCartItemsCountQueryHandler : IRequestHandler<GetCartItemsCountQuery, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        public GetCartItemsCountQueryHandler(IApplicationDbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
        }
        public async Task<int> Handle(GetCartItemsCountQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Cart || Getting cart items count", "Info", "");
            int cartItemCount = await _context.CartItems.Where(ci => ci.UserId == request.UserId).SumAsync(ci => ci.Quantity, cancellationToken);

            if (cartItemCount == 0)
            {
                await _loggerService.LogAsync("Cart || No cart items for user", "Warning", "");

                return 0;
            }

            await _loggerService.LogAsync("Cart || Got cart items count", "Info", "");

            return cartItemCount;
        }
    }
}
