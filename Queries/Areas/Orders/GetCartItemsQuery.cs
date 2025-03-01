using _200SXContact.Models.Areas.Orders;
using MediatR;

public class GetCartItemsQuery(int orderId) : IRequest<List<CartItemModel>?>
{
    public int OrderId { get; set; } = orderId;
}