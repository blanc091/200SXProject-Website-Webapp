using Microsoft.AspNetCore.Mvc;
using _200SXContact.Models;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using Ganss.Xss;
using AngleSharp.Dom;

namespace _200SXContact.Controllers
{
    [Route("contact")]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }
		[HttpPost("send-email")]
		[ValidateAntiForgeryToken]
		public IActionResult SendEmail(ContactForm model, string ViewName)
		{
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
						To = "test@200sxproject.com",
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
					// Log the error
					LogEmailError(model, ex);

					// Set error state
					TempData["IsFormSubmitted"] = true;
					TempData["IsFormSuccess"] = false;
					ViewData["IsFormSubmitted"] = true;
					ViewData["IsFormSuccess"] = false;
					TempData["Message"] = $"Error: {ex.Message}";
				}
			}
			// On failure, return to the original view with the form model
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
				To = "test@200sxproject.com",
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
                var fromAddress = new MailAddress("test@200sxproject.com", "Admin");
                var toAddress = new MailAddress("test@200sxproject.com", "Admin");
                string subject = $"New Contact Form Submission from {model.Name}";
                string body = $"Name: {model.Name}\nEmail: {model.Email}\nMessage: {model.Message}";

                using (var smtpClient = new SmtpClient
                {
                    Host = "mail5019.site4now.net",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential("test@200sxproject.com", "Recall1547!")
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
    }
}
