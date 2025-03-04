using _200SXContact.Data;
using _200SXContact.Models.Areas.Newsletter;
using _200SXContact.Services;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class SubscribeToNewsletterCommandHandler : IRequestHandler<SubscribeToNewsletterCommand, IActionResult>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public SubscribeToNewsletterCommandHandler(ApplicationDbContext context, ILoggerService loggerService, IEmailService emailService, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _loggerService = loggerService;
            _emailService = emailService;
            _configuration = configuration;
            _mapper = mapper;
        }
        public async Task<IActionResult> Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Starting subscribing to the newsletter", "Info", "");

            if (!string.IsNullOrWhiteSpace(request.HoneypotSpam))
            {
                return new BadRequestObjectResult("Spam detected");
            }

            if (string.IsNullOrWhiteSpace(request.RecaptchaResponse) || !await VerifyRecaptchaAsync(request.RecaptchaResponse))
            {
                return new RedirectToActionResult("Index", "Home", new { IsNewsletterError = "yes", Message = "Failed reCAPTCHA validation." });
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                await _loggerService.LogAsync("Newsletter || No email found in request for newsletter subscription", "Error", "");

                return new RedirectToActionResult("Index", "Home", new { IsNewsletterError = "yes", Message = "Email required !" });
            }

            await _loggerService.LogAsync("Starting getting existing subscription when subscribing to the newsletter", "Info", "");

            NewsletterSubscription? existingSubscription = await _context.NewsletterSubscriptions.FirstOrDefaultAsync(sub => sub.Email == request.Email, cancellationToken);

            await _loggerService.LogAsync("Finished getting existing subscription when subscribing to the newsletter", "Info", "");

            if (existingSubscription != null)
            {
                if (!existingSubscription.IsSubscribed)
                {
                    existingSubscription.IsSubscribed = true;
                    existingSubscription.SubscribedAt = DateTime.Now;
                    await _context.SaveChangesAsync(cancellationToken);

                    await _loggerService.LogAsync("Finished subscribing to the newsletter", "Info", "");

                    return new RedirectToActionResult("Index", "Home", new { IsNewsletterSubscribed = "yes", IsNewsletterError = "no", Message = "Email resubscribed successfully !" });
                }
                await _loggerService.LogAsync("Email is already registered when subscribing to the newsletter", "Warning", "");

                return new RedirectToActionResult("Index", "Home", new { IsNewsletterError = "yes", Message = "Email already registered !" });
            }

            NewsletterSubscription newsletter = _mapper.Map<NewsletterSubscription>(request);
            await _context.NewsletterSubscriptions.AddAsync(newsletter, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync("Finished adding newsletter subscription in the DB", "Info", "");

            return new RedirectToActionResult("Index", "Home", new { IsNewsletterSubscribed = "yes", IsNewsletterError = "no", Message = "Subscribed successfully !" });
        }
        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            await _loggerService.LogAsync("Starting verifying recaptcha for subscribing to the newsletter", "Info", "");

            string? secretKey = _configuration["Recaptcha:SecretKey"];
            using (var client = new HttpClient())
            {
                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", secretKey },
                { "response", token }
            });

                await _loggerService.LogAsync("Starting getting Google recaptcha for subscribing to the newslette", "Info", "");

                HttpResponseMessage response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent);

                await _loggerService.LogAsync("Got http response from Google recaptcha for subscribing to the newsletter", "Info", "");

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                    return result.success == true;
                }
            }
            await _loggerService.LogAsync("Finished verifying recaptcha for subscribing to the newsletter", "Info", "");

            return false;
        }
    }
}
