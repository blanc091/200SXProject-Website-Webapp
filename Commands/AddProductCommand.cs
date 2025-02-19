using MediatR;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Commands
{
    public class AddProductCommand : IRequest<bool>
	{
		public Product Product { get; set; }
		public List<IFormFile> Images { get; set; }

		public AddProductCommand(Product product, List<IFormFile> images)
		{
			Product = product;
			Images = images;
		}
	}
}
