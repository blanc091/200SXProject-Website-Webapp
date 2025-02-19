using MediatR;
using _200SXContact.Models.DTOs.Areas.Products;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetAddProductInterfaceQuery : IRequest<ProductDto>
    {
    }
}