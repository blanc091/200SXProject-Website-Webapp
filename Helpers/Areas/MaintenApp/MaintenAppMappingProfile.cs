using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.DTOs.Areas.MaintenApp;

namespace _200SXContact.Helpers.Areas.MaintenApp
{
    public class NewsletterMappingProfile : Profile
    {
        public NewsletterMappingProfile()
        {
            CreateMap<ReminderItem, ReminderItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EntryItem, opt => opt.MapFrom(src => src.EntryItem))
                .ForMember(dest => dest.EntryDescription, opt => opt.MapFrom(src => src.EntryDescription))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.EmailSent, opt => opt.MapFrom(src => src.EmailSent))
                .ReverseMap();
        }
    }
}

