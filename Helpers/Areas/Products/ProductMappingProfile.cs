using AutoMapper;
using _200SXContact.Models.Areas.Products;
using _200SXContact.Models.DTOs.Areas.Products;
using _200SXContact.Interfaces.Areas.Products;

namespace _200SXContact.Helpers.Areas.Products
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.ImagePaths, opt => opt.MapFrom(src => src.ImagePaths ?? new List<string>()))
                .ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => src.DateAdded.ToString("yyyy-MM-dd HH:mm:ss")));

            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.ImagePaths, opt => opt.MapFrom(src => src.ImagePaths ?? new List<string>()))
                .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => DateTime.Parse(src.AddedDate)));
        }
    }
}
