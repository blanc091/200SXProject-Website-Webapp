using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.Admin
{
    public class LoggingMappingProfile : Profile
    {
        public LoggingMappingProfile()
        {
            CreateMap<Logging, LoggingDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LogLevel, opt => opt.MapFrom(src => src.LogLevel))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message)) 
                .ForMember(dest => dest.Exception, opt => opt.MapFrom(src => src.Exception))
                .ReverseMap();

            CreateMap<EmailLog, EmailLogDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.To))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Body))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ErrorMessage, opt => opt.MapFrom(src => src.ErrorMessage))
                .ReverseMap();
        }
    }
}