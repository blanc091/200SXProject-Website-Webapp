using _200SXContact.Models.DTOs.Areas.Products;
using MediatR;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetProductsQuery : IRequest<List<ProductDto?>?> {}
}
