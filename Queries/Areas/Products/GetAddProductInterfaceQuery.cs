using MediatR;
using _200SXContact.Interfaces.Areas.Products;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetAddProductInterfaceQuery : IRequest<IProductDto>
    {
    }
}