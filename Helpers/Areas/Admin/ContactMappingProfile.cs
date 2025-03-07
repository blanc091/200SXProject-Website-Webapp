using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.Admin
{
    public class ContactMappingProfile : Profile
    {
        public ContactMappingProfile()
        {
            CreateMap<ContactFormDto, ContactForm>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ReverseMap();
        }
    }
}
