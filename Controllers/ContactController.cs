using Microsoft.AspNetCore.Mvc;
using _200SXContact.Models;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using Ganss.Xss;
using AngleSharp.Dom;
using _200SXContact.Models.Configs;
using Microsoft.Extensions.Options;
using System.Net;
using _200SXContact.Services;

namespace _200SXContact.Controllers
{
	public class ContactController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly NetworkCredential _credentials;
		private readonly IConfiguration _configuration;
		private readonly ILoggerService _loggerService;
		public ContactController(ApplicationDbContext context, IOptions<AppSettings> appSettings, IConfiguration configuration, ILoggerService loggerService)
		{
			var emailSettings = appSettings.Value.EmailSettings;
			_context = context;
			_credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
			_configuration = configuration;
			_loggerService = loggerService;
		}
        [HttpPost]
        [Route("contact/send-email")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(ContactForm model, string viewName, string honeypotSpamContact, string gRecaptchaResponseContact)
        {
            try
            {
                ModelState.Remove("honeypotSpamContact");
                await _loggerService.LogAsync("Contact form || Started sending contact email from contact form", "Info", "");
                if (!string.IsNullOrWhiteSpace(honeypotSpamContact))
                {
                    return BadRequest("Spam detected");
                }
                if (string.IsNullOrWhiteSpace(gRecaptchaResponseContact) || !await VerifyRecaptchaAsync(gRecaptchaResponseContact))
                {
                    return ReturnWithError(viewName, model, "Failed reCAPTCHA validation.");
                }
                if (!ModelState.IsValid)
                {
                    await _loggerService.LogAsync("Contact form || Invalid form submission in contact form", "Info", "");
                    return ReturnWithError(viewName, model, "Invalid form submission.");
                }
                await _loggerService.LogAsync("Contact form || Sanitizing and sending contact email", "Info", "");
                var sanitizer = new HtmlSanitizer();
                model.Message = sanitizer.Sanitize(model.Message);
                await SendEmailToAdmin(model);
                await _loggerService.LogAsync("Contact form || Send email to admin from contact form", "Info", "");
                await LogEmail(model, "Sent");
                return ReturnWithSuccess(viewName, "Message sent successfully !");
            }
            catch (Exception ex)
            {
                await LogEmail(model, "Failed", ex.Message);
                await _loggerService.LogAsync("Contact form || Error in sending contact email to admin " + ex.Message, "Error", "");
                return ReturnWithError(viewName, model, $"Error: {ex.Message}");
            }
        }
        private IActionResult ReturnWithError(string viewName, ContactForm model, string errorMessage)
        {
            TempData["IsFormSubmitted"] = true;
            TempData["IsFormSuccess"] = false;
            TempData["Message"] = errorMessage;
            _loggerService.LogAsync("Contact form || Returning view with error model for contact form", "Error", "");
            return View($"~/Views/{viewName}.cshtml", model);
        }
        private IActionResult ReturnWithSuccess(string viewName, string successMessage)
        {
            TempData["IsFormSubmitted"] = true;
            TempData["IsFormSuccess"] = true;
            TempData["Message"] = successMessage;
            _loggerService.LogAsync("Contact form || Returning view with success for contact form", "Error", "");
            return View($"~/Views/{viewName}.cshtml");
        }
        private async Task LogEmail(ContactForm model, string status, string errorMessage = null)
        {
            await _loggerService.LogAsync($"Contact form || Logging email with status: {status}", "Info", "");
            var emailLog = new EmailLog
            {
                Timestamp = DateTime.Now,
                From = model.Email,
                To = _credentials.UserName,
                Subject = $"New Contact Form Submission from {model.Name}",
                Body = model.Message,
                Status = status,
                ErrorMessage = errorMessage
            };
            await _context.EmailLogs.AddAsync(emailLog);
            await _context.SaveChangesAsync();
            await _loggerService.LogAsync($"Contact form || Email logged with status: {status}", "Info", "");
        }
        private async Task SendEmailToAdmin(ContactForm model)
		{
			try
			{
                await _loggerService.LogAsync("Contact form || Started sending contact email to admin", "Info", "");
                var fromAddress = new MailAddress(_credentials.UserName, model.Email);
				var toAddress = new MailAddress(_credentials.UserName, "Admin");
				string subject = $"New Contact Form Submission from {model.Name}";
				string body = $"Name: {model.Name}\nEmail: {model.Email}\nMessage: {model.Message}";
				using (var smtpClient = new SmtpClient
				{
					Host = "mail5019.site4now.net",
					Port = 587,
					EnableSsl = true,
					Credentials = new System.Net.NetworkCredential(_credentials.UserName, _credentials.Password)
				})
				{
					using (var message = new MailMessage(fromAddress, toAddress)
					{
						Subject = subject,
						Body = body
					})
					{
                        await _loggerService.LogAsync("Contact form || Email built, sending", "Info", "");
                        smtpClient.Send(message);
					}
				}
                await _loggerService.LogAsync("Contact form || Sent contact email to admin", "Info", "");
            }
			catch (SmtpException ex)
			{
				var errorMessage = $"SMTP Error: {ex.Message}\n" +
								   $"StatusCode: {ex.StatusCode}\n" +
								   $"InnerException: {ex.InnerException?.Message}";
                await _loggerService.LogAsync("Contact form || Failed to send email to the admin. Please try again later" + ex.Message, "Error", "");
                throw new Exception("Failed to send email to the admin. Please try again later", ex);
			}
			catch (Exception ex)
			{
                await _loggerService.LogAsync("Contact form || An unexpected error occurred while sending the email" + ex.Message, "Error", "");
                throw new Exception("An unexpected error occurred while sending the email", ex);
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
	}
}