using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetOrderSummaryQuery(int orderId) : IRequest<OrderUserDashDto?>
    {
        public int OrderId { get; } = orderId;
    }
}
