using _200SXContact.Models.Areas.Orders;
using MediatR;

public class GetCartItemsQuery(int orderId) : IRequest<List<CartItem>?>
{
    public int OrderId { get; set; } = orderId;
}