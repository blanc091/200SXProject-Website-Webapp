using MediatR;

namespace _200SXContact.Commands.Areas.Orders
{
    public class RemoveFromCartCommand : IRequest<bool>
    {
        public int ProductId { get; set; }
        public required string UserId { get; set; }
    }
}
