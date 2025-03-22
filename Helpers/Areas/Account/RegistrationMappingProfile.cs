using _200SXContact.Commands.Areas.Account;
using _200SXContact.Models.DTOs.Areas.Account;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.Account
{
    public class RegistrationMappingProfile : Profile
    {
        public RegistrationMappingProfile()
        {
            CreateMap<ExtendedRegisterDto, RegisterUserCommand>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.SubscribeToNewsletter, opt => opt.MapFrom(src => src.SubscribeToNewsletter))
                .ForMember(dest => dest.HoneypotSpam, opt => opt.MapFrom(src => src.honeypotSpam))
                .ForMember(dest => dest.RecaptchaResponse, opt => opt.MapFrom(src => src.RecaptchaResponse))
                .ForMember(dest => dest.TimeZoneCookie, opt => opt.MapFrom(src => src.TimeZoneCookie))
                .ReverseMap();
        }
    }
}
