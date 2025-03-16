using _200SXContact.Models.Areas.Chat;
using _200SXContact.Models.DTOs.Areas.Chat;
using AutoMapper;

namespace _200SXContact.Helpers.Areas.Chat
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<ChatMessage, ChatMessageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt))
                .ReverseMap();
        }
    }
}
