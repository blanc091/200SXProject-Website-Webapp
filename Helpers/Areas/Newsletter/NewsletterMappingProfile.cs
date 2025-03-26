using _200SXContact.Commands.Areas.Newsletter;
using _200SXContact.Helpers;
using _200SXContact.Models.Areas.Newsletter;
using _200SXContact.Models.DTOs.Areas.Newsletter;
using Microsoft.AspNetCore.Http;

namespace _200SXContact.Helpers.Areas.Newsletter
{
    public class NewsletterMappingProfile : Profile
    {
        public NewsletterMappingProfile()
        {
            CreateMap<NewsletterViewModel, NewsletterDto>()
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Body))
                .ReverseMap();

            CreateMap<NewsletterSubscription, NewsletterSubscriptionDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.IsSubscribed, opt => opt.MapFrom(src => src.IsSubscribed))
               .ForMember(dest => dest.SubscribedAt, opt => opt.MapFrom(src => src.SubscribedAt));

            CreateMap<SubscribeToNewsletterCommand, NewsletterSubscription>()
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.IsSubscribed, opt => opt.MapFrom(src => true))
               .ForMember(dest => dest.SubscribedAt, opt => opt.MapFrom<ClientTimeResolver<SubscribeToNewsletterCommand, NewsletterSubscription>>());
        }
    }
}