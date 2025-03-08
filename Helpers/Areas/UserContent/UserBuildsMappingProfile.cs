using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.UserContent;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.UserContent
{
    public class UserBuildsMappingProfile : Profile
    {
        public UserBuildsMappingProfile()
        {
            CreateMap<BuildsComments, BuildsCommentsDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.UserBuildId, opt => opt.MapFrom(src => src.UserBuildId))
            .ForMember(dest => dest.UserBuild, opt => opt.MapFrom(src => src.UserBuild))
            .ReverseMap();
        }
    }
}