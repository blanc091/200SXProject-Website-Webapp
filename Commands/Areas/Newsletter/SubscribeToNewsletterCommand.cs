using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Newsletter;

namespace _200SXContact.Commands.Areas.Newsletter
{
    public class SubscribeToNewsletterCommand : IRequest<IActionResult>
    {       
        public required string Email { get; set; }
        public required string HoneypotSpam { get; set; }
        public required string RecaptchaResponse { get; set; }
        public required bool IsCalledFromRegisterForm { get; set; }
    }
    public class SubscribeToNewsletterCommandHandler : IRequestHandler<SubscribeToNewsletterCommand, IActionResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IClientTimeProvider _clientTimeProvider;
        public SubscribeToNewsletterCommandHandler(IClientTimeProvider clientTimeProvider, IApplicationDbContext context, ILoggerService loggerService, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _loggerService = loggerService;
            _mapper = mapper;
            _configuration = configuration;
            _clientTimeProvider = clientTimeProvider;
        }
        public async Task<IActionResult> Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Newsletter || Started subscribing to newsletter for " + request.Email, "Info", "");

            if (!string.IsNullOrWhiteSpace(request.HoneypotSpam))
            {
                return new BadRequestObjectResult("Spam detected");
            }

            if (!request.IsCalledFromRegisterForm && (string.IsNullOrWhiteSpace(request.RecaptchaResponse) || !await VerifyRecaptchaAsync(request.RecaptchaResponse)))
            {
                return new RedirectToActionResult("Index", "Home", new { IsNewsletterError = "yes", Message = "Failed reCAPTCHA validation." });
            }

            SubscribeNewsletterCommandValidator validator = new SubscribeNewsletterCommandValidator();
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                string errorMessage = validationResult.Errors.First().ErrorMessage;

                await _loggerService.LogAsync("Newsletter || " + errorMessage, "Error", "");

                return new RedirectToActionResult("Index", "Home", new { IsNewsletterError = "yes", Message = errorMessage });
            }

            NewsletterSubscription? existingSubscription = await _context.NewsletterSubscriptions.FirstOrDefaultAsync(sub => sub.Email == request.Email, cancellationToken);
            DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();

            if (existingSubscription != null)
            {
                if (!existingSubscription.IsSubscribed)
                {
                    existingSubscription.IsSubscribed = true;
                    existingSubscription.SubscribedAt = clientTime;
                    await _context.SaveChangesAsync(cancellationToken);

                    await _loggerService.LogAsync("Newsletter || Resubscribed for newsletter for " + request.Email, "Info", "");

                    return new RedirectToActionResult("Index", "Home", new { IsNewsletterSubscribed = "yes", IsNewsletterError = "no", Message = "Email resubscribed successfully !" });
                }

                return new RedirectToActionResult("Index", "Home", new { IsNewsletterError = "yes", Message = "Email already registered !" });
            }

            NewsletterSubscription subscription = _mapper.Map<NewsletterSubscription>(request);
            subscription.SubscribedAt = clientTime;
            await _context.NewsletterSubscriptions.AddAsync(subscription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync("Newsletter || Subscribed to newsletter for " + request.Email, "Info", "");

            return new RedirectToActionResult("Index", "Home", new { IsNewsletterSubscribed = "yes", IsNewsletterError = "no", Message = "Subscribed successfully !" });
        }
        public class SubscribeNewsletterCommandValidator : AbstractValidator<SubscribeToNewsletterCommand>
        {
            public SubscribeNewsletterCommandValidator()
            {
                RuleFor(x => x.Email).NotEmpty().WithMessage("Please enter a valid email address !").Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Please enter a valid email address !").MaximumLength(50).WithMessage("Email cannot exceed 50 characters !");
            }
        }
        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            string? secretKey = _configuration["Recaptcha:SecretKey"];
            using (HttpClient client = new HttpClient())
            {
                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", secretKey },
                { "response", token }
            });

                HttpResponseMessage response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

                    return result.success == true;
                }
            }
            return false;
        }
    }
}
