using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Configs;
using System.Net.Mail;
using System.Net;
using _200SXContact.Interfaces.Areas.Data;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class SendNewsletterCommand : IRequest<Unit>
    {
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
    public class SendNewsletterCommandHandler : IRequestHandler<SendNewsletterCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly NetworkCredential _credentials;
        public SendNewsletterCommandHandler(IApplicationDbContext context, ILoggerService loggerService, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _loggerService = loggerService;
            EmailSettings emailSettings = appSettings.Value.EmailSettings;
            _credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
        }
        public async Task<Unit> Handle(SendNewsletterCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Newsletter || Started sending newsletter", "Info", "");

            List<string> subscribers = _context.NewsletterSubscriptions.Where(sub => sub.IsSubscribed).Select(sub => sub.Email).ToList();

            foreach (string email in subscribers)
            {
                SendEmailToSubscriber(email, request.Subject, request.Body);
            }

            await _loggerService.LogAsync("Newsletter || Finished sending newsletter", "Info", "");

            return Unit.Value;
        }
        [Authorize(Roles = "Admin")]
        private void SendEmailToSubscriber(string email, string subject, string body)
        {
            _loggerService.LogAsync("Newsletter || Started sending newsletter email to subscriber admin", "Info", "");

            body = body.Replace("{EMAIL}", WebUtility.UrlEncode(email));
            SmtpClient smtpClient = new SmtpClient("mail5019.site4now.net")
            {
                Port = 587,
                Credentials = new NetworkCredential(_credentials.UserName, _credentials.Password),
                EnableSsl = true,
            };
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(_credentials.UserName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _loggerService.LogAsync($"Newsletter || Failed to send email to {email}: {ex.Message}", "Error", "");
            }

            _loggerService.LogAsync("Newsletter || Finished sending newsletter email to subscriber admin", "Info", "");
        }
    }
}
