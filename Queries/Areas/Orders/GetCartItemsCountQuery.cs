using MediatR;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetCartItemsCountQuery : IRequest<int>
    {
        public required string UserId { get; set; }
    }
}
