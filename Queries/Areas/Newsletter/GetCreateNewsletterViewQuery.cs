using _200SXContact.Models.Areas.Newsletter;
using _200SXContact.Models.DTOs.Areas.Newsletter;
using MediatR;

namespace _200SXContact.Queries.Areas.Newsletter
{
    public class GetCreateNewsletterViewQuery : IRequest<NewsletterDto> {}
}
