using MediatR;
using _200SXContact.Models.DTOs.Areas.Products;

namespace _200SXContact.Commands.Areas.Products
{
    public class AddProductCommand(ProductDto product, List<IFormFile> images) : IRequest<bool>
	{
        public ProductDto Product { get; set; } = product;
        public List<IFormFile> Images { get; set; } = images;
    }
}
