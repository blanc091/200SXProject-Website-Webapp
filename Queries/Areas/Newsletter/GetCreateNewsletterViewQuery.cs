using _200SXContact.Models;
using MediatR;

namespace _200SXContact.Queries.Areas.Newsletter
{
    public class GetCreateNewsletterViewQuery : IRequest<NewsletterViewModel> {}
}
