using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetCartItemsCheckoutQuery(string userId) : IRequest<List<CartItemDto?>?>
    {
        public string UserId { get; } = userId;
    }
}
