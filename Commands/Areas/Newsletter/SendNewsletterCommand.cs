using MediatR;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class SendNewsletterCommand : IRequest<Unit>
    {
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
