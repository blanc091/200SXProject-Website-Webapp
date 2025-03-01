using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;

public class GetUserOrdersQuery(string userId) : IRequest<List<OrderUserDashDto>?>
{
    public string UserId { get; set; } = userId;
}
