using _200SXContact.Models.Areas.Chat;
using _200SXContact.Models.DTOs.Areas.Chat;

namespace _200SXContact.Helpers.Areas.Chat
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<ChatMessage, ChatMessageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt))              
                .ReverseMap();

            CreateMap<ChatSession, ChatSessionDto>()
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.ConnectionId, opt => opt.MapFrom(src => src.ConnectionId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.IsAnswered, opt => opt.MapFrom(src => src.IsAnswered))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => src.LastUpdatedAt))
                .ReverseMap();
        }
    }
}

