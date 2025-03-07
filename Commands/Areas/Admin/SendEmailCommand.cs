using _200SXContact.Helpers.Areas.Admin;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;
using _200SXContact.Services;
using AutoMapper;
using Ganss.Xss;
using MediatR;
using System.Net;

namespace _200SXContact.Commands.Areas.Admin
{
    public class SendEmailCommand : IRequest<ContactResult>
    {
        public ContactFormDto Model { get; init; }
        public string ViewName { get; init; }
        public string HoneypotSpamContact { get; init; }
        public string GRecaptchaResponseContact { get; init; }
    }

    public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, ContactResult>
    {
        private readonly ILoggerService _loggerService;
        private readonly IEmailService _emailService;
        private readonly IHtmlSanitizer _htmlSanitizer;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly NetworkCredential _credentials;
        public SendEmailCommandHandler(ILoggerService loggerService, NetworkCredential credentials, IConfiguration configuration, IEmailService emailService, IHtmlSanitizer htmlSanitizer, IMapper mapper)
        {
            _loggerService = loggerService;
            _emailService = emailService;
            _htmlSanitizer = htmlSanitizer;
            _mapper = mapper;
            _configuration = configuration;
            _credentials = credentials;
        }
        public async Task<ContactResult> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            var model = _mapper.Map<ContactForm>(request.Model);
            var viewName = request.ViewName;

            try
            {
                await _loggerService.LogAsync("Contact form || Started sending contact email", "Info", "");

                if (!string.IsNullOrWhiteSpace(request.HoneypotSpamContact))
                    return ContactResult.Failure(viewName, model, "Spam detected");

                if (string.IsNullOrWhiteSpace(request.GRecaptchaResponseContact) || !await VerifyRecaptchaAsync(request.GRecaptchaResponseContact))
                    return ContactResult.Failure(viewName, model, "Failed reCAPTCHA validation.");

                if (!IsValidModel(model))
                {
                    await _loggerService.LogAsync("Contact form || Invalid form submission", "Info", "");
                    return ContactResult.Failure(viewName, model, "Invalid form submission.");
                }

                await _loggerService.LogAsync("Contact form || Sanitizing and sending email", "Info", "");
                model.Message = _htmlSanitizer.Sanitize(model.Message);

                await _emailService.SendEmailToAdmin(model);
                await _loggerService.LogAsync("Contact form || Email sent to admin", "Info", "");

                return ContactResult.Success(viewName, "Message sent successfully!");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Contact form || Error: {ex.Message}", "Error", "");
                return ContactResult.Failure(viewName, model, $"Error: {ex.Message}");
            }
        }        
        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            var secretKey = _configuration["Recaptcha:SecretKey"];
            using (var client = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", token }
                });
                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"reCAPTCHA Response: {jsonString}");
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
        private bool IsValidModel(ContactForm model)
        {
            return model != null && !string.IsNullOrWhiteSpace(model.Email) && !string.IsNullOrWhiteSpace(model.Message);
        }
    }
}
