using _200SXContact.Interfaces.Areas.Products;
using _200SXContact.Models.Areas.Products;
using _200SXContact.Models.DTOs.Areas.Products;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.Products
{
    public class ImagePathsResolver : IValueResolver<ProductDto, Product, List<string>>
    {
        public List<string> Resolve(ProductDto source, Product destination, List<string> destMember, ResolutionContext context)
        {
			return string.IsNullOrEmpty(source.ImagePaths) ? new List<string>() : source.ImagePaths.Split(',').ToList();
		}
    }
}
