using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;

namespace _200SXContact.Commands.Areas.Orders
{
    public class AddToCartCommand(int productId, int quantity, string userId) : IRequest<CartItemDto?>
    {
        public int ProductId { get; } = productId;
        public int Quantity { get; } = quantity;
        public string UserId { get; } = userId;
    }
}
