using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;

namespace _200SXContact.Commands.Areas.Orders
{
    public class UpdateOrderTrackingCommand(OrderTrackingUpdateDto updateDto) : IRequest<bool>
    {
        public OrderTrackingUpdateDto UpdateDto { get; } = updateDto;
    }
}
