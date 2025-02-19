using MediatR;
using _200SXContact.Models.DTOs.Areas.Products;

namespace _200SXContact.Commands
{
    public class AddProductCommand : IRequest<bool>
	{
		public ProductDto Product { get; set; }
		public List<IFormFile> Images { get; set; }

		public AddProductCommand(ProductDto product, List<IFormFile> images)
		{
			Product = product;
			Images = images;
		}
	}
}
