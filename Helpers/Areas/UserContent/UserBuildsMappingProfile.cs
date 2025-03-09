using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.UserContent;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.UserContent
{
    public class UserBuildsMappingProfile : Profile
    {
        public UserBuildsMappingProfile()
        {
            CreateMap<BuildComment, BuildCommentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserBuildId, opt => opt.MapFrom(src => src.UserBuildId))
                .ForMember(dest => dest.UserBuild, opt => opt.MapFrom(src => src.UserBuild))
                .ReverseMap();

            CreateMap<UserBuild, UserBuildDto>()
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ImagePaths, opt => opt.MapFrom(src => src.ImagePaths))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ReverseMap();
        }
    }
}