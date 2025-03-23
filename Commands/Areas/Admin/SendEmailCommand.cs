using _200SXContact.Helpers.Areas.Admin;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.Configs;
using _200SXContact.Models.DTOs.Areas.Admin;
using Ganss.Xss;
using System.Net;
using System.Net.Mail;

namespace _200SXContact.Commands.Areas.Admin
{
    public class SendEmailCommand : IRequest<ContactResult>
    {
        public required ContactFormDto Model { get; init; }
        public required string ViewName { get; init; }
        public required string HoneypotSpamContact { get; init; }
        public required string GRecaptchaResponseContact { get; init; }
    }
    public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, ContactResult>
    {
        private readonly ILoggerService _loggerService;
        private readonly IEmailService _emailService;
        private readonly IHtmlSanitizer _htmlSanitizer;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly NetworkCredential _credentials;
        public SendEmailCommandHandler(ILoggerService loggerService, IOptions<AppSettings> appSettings, NetworkCredential credentials, IConfiguration configuration, IEmailService emailService, IHtmlSanitizer htmlSanitizer, IMapper mapper)
        {
            _loggerService = loggerService;
            _emailService = emailService;
            _htmlSanitizer = htmlSanitizer;
            _mapper = mapper;
            _configuration = configuration;
            EmailSettings emailSettings = appSettings.Value.EmailSettings;
            _credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
        }
        public async Task<ContactResult> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            ContactForm model = _mapper.Map<ContactForm>(request.Model);
            string viewName = request.ViewName;
            try
            {
                await _loggerService.LogAsync("Contact form || Started sending contact email", "Info", "");

                if (!string.IsNullOrWhiteSpace(request.HoneypotSpamContact))
                    return ContactResult.Failure(viewName, model, "Spam detected");

                if (string.IsNullOrWhiteSpace(request.GRecaptchaResponseContact) || !await VerifyRecaptchaAsync(request.GRecaptchaResponseContact))
                    return ContactResult.Failure(viewName, model, "Failed reCAPTCHA validation.");

                ContactResult? validationResult = await IsValidModel(model, viewName);

                if (validationResult != null)
                {
                    return validationResult;
                }

                await _loggerService.LogAsync("Contact form || Sanitizing and sending email", "Info", "");

                model.Message = _htmlSanitizer.Sanitize(model.Message);

                await _emailService.SendEmailToAdmin(model);

                await _loggerService.LogAsync("Contact form || Email sent to admin", "Info", "");

                return ContactResult.Success(viewName, "Message sent successfully !");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Contact form || Error: {ex.Message}", "Error", "");

                return ContactResult.Failure(viewName, model, $"Error: {ex.Message}");
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
                else
                {
                    Console.WriteLine($"Failed to verify reCAPTCHA: {response.StatusCode}");
                }
            }

            return false;
        }
        private async Task<ContactResult?> IsValidModel(ContactForm model, string viewName)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                await _loggerService.LogAsync("Contact form || Name required", "Info", "");

                return ContactResult.Failure(viewName, model, "Name is required !");
            }

            if (model.Name.Length > 150)
            {
                await _loggerService.LogAsync("Contact form || Name too long", "Info", "");

                return ContactResult.Failure(viewName, model, "Name cannot be longer than 150 characters !");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                await _loggerService.LogAsync("Contact form || Email is required", "Info", "");

                return ContactResult.Failure(viewName, model, "Email is required !");
            }

            try
            {
                MailAddress mailAddress = new MailAddress(model.Email);

                if (mailAddress.Address != model.Email)
                {
                    await _loggerService.LogAsync("Contact form || Invalid email format", "Info", "");

                    return ContactResult.Failure(viewName, model, "Please enter a valid email address !");
                }
            }
            catch (FormatException)
            {
                await _loggerService.LogAsync("Contact form || Invalid email format", "Info", "");

                return ContactResult.Failure(viewName, model, "Please enter a valid email address !");
            }

            if (string.IsNullOrWhiteSpace(model.Message))
            {
                await _loggerService.LogAsync("Contact form || Message is required", "Info", "");

                return ContactResult.Failure(viewName, model, "Message required !");
            }

            if (model.Message.Length > 10000)
            {
                await _loggerService.LogAsync("Contact form || Message is too long", "Info", "");

                return ContactResult.Failure(viewName, model, "Message cannot be longer than 10,000 characters.");
            }

            return null;
        }
    }
}
