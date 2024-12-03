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
namespace _200SXContact.Controllers
{
	public class ContactController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly NetworkCredential _credentials;
		private readonly IConfiguration _configuration;
		public ContactController(ApplicationDbContext context, IOptions<AppSettings> appSettings, IConfiguration configuration)
		{
			var emailSettings = appSettings.Value.EmailSettings;
			_context = context;
			_credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
			_configuration = configuration;
		}
		[HttpPost]
		[Route("contact/send-email")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendEmail(ContactForm model, string ViewName, string honeypotSpamContact, string gRecaptchaResponseContact)
		{
			ModelState.Remove("honeypotSpamContact");
			if (!string.IsNullOrWhiteSpace(honeypotSpamContact))
			{
				return BadRequest("Spam detected");
			}
			if (string.IsNullOrWhiteSpace(gRecaptchaResponseContact) || !await VerifyRecaptchaAsync(gRecaptchaResponseContact))
			{
				TempData["IsFormSubmitted"] = "no";
				TempData["IsFormError"] = "yes";
				TempData["Message"] = "Failed reCAPTCHA validation.";
				return View("~/Views/Home/Index.cshtml");
			}
			if (ModelState.IsValid)
			{
				var sanitizer = new HtmlSanitizer();
				model.Message = sanitizer.Sanitize(model.Message);
				try
				{
					SendEmailToAdmin(model);
					var emailLog = new EmailLog
					{
						Timestamp = DateTime.Now,
						From = model.Email,
						To = _credentials.UserName,
						Subject = $"New Contact Form Submission from {model.Name}",
						Body = model.Message,
						Status = "Sent",
						ErrorMessage = null
					};
					_context.EmailLogs.Add(emailLog);
					_context.SaveChanges();
					TempData["Message"] = "Message sent successfully!";
					TempData["IsFormSubmitted"] = true;
					TempData["IsFormSuccess"] = true;
					ViewData["IsFormSubmitted"] = true;
					ViewData["IsFormSuccess"] = true;
					string viewPathSuccess = $"~/Views/{ViewName}.cshtml";
					return View(viewPathSuccess);
				}
				catch (Exception ex)
				{
					LogEmailError(model, ex);
					TempData["IsFormSubmitted"] = true;
					TempData["IsFormSuccess"] = false;
					ViewData["IsFormSubmitted"] = true;
					ViewData["IsFormSuccess"] = false;
					TempData["Message"] = $"Error: {ex.Message}";
				}
			}
			TempData["IsFormSubmitted"] = true;
			TempData["IsFormSuccess"] = false;
			ViewData["IsFormSubmitted"] = true;
			ViewData["IsFormSuccess"] = false;
			string viewPathFail = $"~/Views/{ViewName}.cshtml";
			return View(viewPathFail, model);
		}
		private void LogEmailError(ContactForm model, Exception ex)
		{
			var emailLog = new EmailLog
			{
				Timestamp = DateTime.Now,
				From = model.Email,
				To = _credentials.UserName,
				Subject = $"New Contact Form Submission from {model.Name}",
				Body = model.Message,
				Status = "Failed",
				ErrorMessage = ex.Message
			};
			_context.EmailLogs.Add(emailLog);
			_context.SaveChanges();
		}
		private void SendEmailToAdmin(ContactForm model)
		{
			try
			{
				var fromAddress = new MailAddress(_credentials.UserName, "Admin");
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
						smtpClient.Send(message);
					}
				}
			}
			catch (SmtpException ex)
			{
				var errorMessage = $"SMTP Error: {ex.Message}\n" +
								   $"StatusCode: {ex.StatusCode}\n" +
								   $"InnerException: {ex.InnerException?.Message}";
				throw new Exception("Failed to send email to the admin. Please try again later.", ex);
			}
			catch (Exception ex)
			{
				throw new Exception("An unexpected error occurred while sending the email.", ex);
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