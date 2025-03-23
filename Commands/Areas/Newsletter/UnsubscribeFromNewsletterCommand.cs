﻿using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Newsletter;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class UnsubscribeFromNewsletterCommand : IRequest
    {
        public required string Email { get; set; }
    }
    public class UnsubscribeFromNewsletterCommandHandler : IRequestHandler<UnsubscribeFromNewsletterCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        public UnsubscribeFromNewsletterCommandHandler(IApplicationDbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
        }
        public async Task Handle(UnsubscribeFromNewsletterCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Newsletter || Starting unsubscribe request for newsletter subscription", "Info", "");

            if (string.IsNullOrEmpty(request.Email))
            {
                await _loggerService.LogAsync("Newsletter || Email not provided for unsubscribe request", "Error", "");

                throw new ArgumentException("Email is required.");
            }

            NewsletterSubscription? subscription = await _context.NewsletterSubscriptions.FirstOrDefaultAsync(sub => sub.Email == request.Email, cancellationToken);

            if (subscription == null || !subscription.IsSubscribed)
            {
                await _loggerService.LogAsync("Newsletter || Email not subscribed and unsubscribe request received", "Error", "");

                throw new InvalidOperationException("Not subscribed.");
            }

            subscription.IsSubscribed = false;
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync("Newsletter || Finished unsubscribe request for newsletter subscription", "Info", "");
        }
    }
}
