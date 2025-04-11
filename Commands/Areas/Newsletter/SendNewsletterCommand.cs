using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Configs;
using System.Net.Mail;
using System.Net;
using _200SXContact.Interfaces.Areas.Data;
using static _200SXContact.Commands.Areas.Newsletter.SendNewsletterCommandHandler;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class SendNewsletterCommand : IRequest<SendNewsletterCommandResult>
    {
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
    public class SendNewsletterCommandHandler : IRequestHandler<SendNewsletterCommand, SendNewsletterCommandResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly NetworkCredential _credentials;
        private readonly IEmailService _emailService;
        public SendNewsletterCommandHandler(IApplicationDbContext context, ILoggerService loggerService, IOptions<AppSettings> appSettings, IEmailService emailService)
        {
            _context = context;
            _loggerService = loggerService;
            EmailSettings emailSettings = appSettings.Value.EmailSettings;
            _credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
            _emailService = emailService;
        }
        public async Task<SendNewsletterCommandResult> Handle(SendNewsletterCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Newsletter || Started sending newsletter", "Info", "");

            SendNewsletterCommandValidator validator = new SendNewsletterCommandValidator();
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                string[] errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();

                await _loggerService.LogAsync("Newsletter || Validation error: " + string.Join(", ", errors), "Error", "");

                return new SendNewsletterCommandResult
                {
                    Succeeded = false,
                    Errors = errors
                };
            }

            List<string> subscribers = _context.NewsletterSubscriptions.Where(sub => sub.IsSubscribed).Select(sub => sub.Email).ToList();

            foreach (string email in subscribers)
            {
                await _emailService.SendEmailToSubscriberAsync(email, request.Subject, request.Body);
            }

            await _loggerService.LogAsync("Newsletter || Finished sending newsletter", "Info", "");

            return new SendNewsletterCommandResult
            {
                Succeeded = true,
                Errors = Array.Empty<string>()
            };
        }
        public class SendNewsletterCommandValidator : AbstractValidator<SendNewsletterCommand>
        {
            public SendNewsletterCommandValidator()
            {
                RuleFor(x => x.Subject).NotEmpty().WithMessage("Subject is required !").MaximumLength(100).WithMessage("Subject cannot exceed 100 characters !");
            }
        }
        public class SendNewsletterCommandResult
        {
            public bool Succeeded { get; set; }
            public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
        }
        
    }
}
