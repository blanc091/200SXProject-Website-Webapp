using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class SubscribeToNewsletterCommand : IRequest<IActionResult>
    {       
        public required string Email { get; set; }
        public required string HoneypotSpam { get; set; }
        public required string RecaptchaResponse { get; set; }
    }
}
