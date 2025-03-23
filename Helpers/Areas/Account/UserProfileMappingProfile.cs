using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.Account;

namespace _200SXContact.Helpers.Areas.Account
{
    public class UserProfileMappingProfile : Profile
    {
        public UserProfileMappingProfile()
        {
            CreateMap<User, UserProfileDto>()
               .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
               .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
               .ReverseMap();
        }
    }
}
