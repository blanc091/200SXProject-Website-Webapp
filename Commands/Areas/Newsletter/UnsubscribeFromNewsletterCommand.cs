using MediatR;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class UnsubscribeFromNewsletterCommand : IRequest
    {
        public required string Email { get; set; }
    }
}
