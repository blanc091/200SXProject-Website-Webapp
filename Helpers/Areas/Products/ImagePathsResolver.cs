using _200SXContact.Interfaces.Areas.Products;
using _200SXContact.Models.Areas.Products;
using _200SXContact.Models.DTOs.Areas.Products;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.Products
{
    public class ImagePathsResolver : IValueResolver<ProductDto, Product, List<string>>
    {
		public List<string> Resolve(ProductDto source, Product destination, List<string> member, ResolutionContext context)
		{
			//if (string.IsNullOrEmpty(source.ImagePaths))
			//	return new List<string>();

			return source.ImagePaths;
		}
	}
}
