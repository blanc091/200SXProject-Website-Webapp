using _200SXContact.Interfaces.Areas.Products;
using _200SXContact.Models.Areas.Products;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.Products
{
    public class ImagePathsResolver : IValueResolver<IProductDto, Product, List<string>>
    {
        public List<string> Resolve(IProductDto source, Product destination, List<string> destMember, ResolutionContext context)
        {
            return source.ImagePaths?.Split(',').ToList() ?? new List<string>();
        }
    }
}
