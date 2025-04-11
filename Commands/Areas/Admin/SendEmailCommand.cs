using _200SXContact.Helpers.Areas.Admin;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.Configs;
using _200SXContact.Models.DTOs.Areas.Admin;
using Ardalis.GuardClauses;
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
                {
                    return ContactResult.Failure(viewName, model, "Spam detected");
                }

                if (string.IsNullOrWhiteSpace(request.GRecaptchaResponseContact) || !await VerifyRecaptchaAsync(request.GRecaptchaResponseContact))
                { 
                    return ContactResult.Failure(viewName, model, "Failed reCAPTCHA validation.");
                }

                ContactResult? validationResult = await IsValidModel(model, viewName);

                if (validationResult != null)
                {
                    return validationResult;
                }

                await _loggerService.LogAsync("Contact form || Sanitizing and sending email", "Info", "");

                model.Message = _htmlSanitizer.Sanitize(model.Message);

                await _emailService.SendEmailToAdminAsync(model);

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
            try
            {
                Guard.Against.NullOrWhiteSpace(model.Name, nameof(model.Name), "Name is required !");

                if (model.Name.Length > 150)
                { 
                    throw new ArgumentException("Name cannot be longer than 150 characters !");
                }

                Guard.Against.NullOrWhiteSpace(model.Email, nameof(model.Email), "Email is required !");

                try
                {
                    var mailAddress = new MailAddress(model.Email);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Please enter a valid email address !");
                }

                Guard.Against.NullOrWhiteSpace(model.Message, nameof(model.Message), "Message is required !");

                if (model.Message.Length > 10000)
                { 
                    throw new ArgumentException("Message cannot be longer than 10,000 characters !");
                }

                return null;
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Contact form || Validation failed: {ex.Message}", "Error", "");

                return ContactResult.Failure(viewName, model, ex.Message);
            }
        }
    }
}
