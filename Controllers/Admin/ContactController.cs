using Microsoft.AspNetCore.Mvc;
using _200SXContact.Models;
using System.Net.Mail;
using _200SXContact.Data;
using Ganss.Xss;
using _200SXContact.Models.Configs;
using Microsoft.Extensions.Options;
using System.Net;
using _200SXContact.Services;
using _200SXContact.Models.Areas.Admin;
using MediatR;
using _200SXContact.Commands.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;

namespace _200SXContact.Controllers.Admin
{
	public class ContactController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly NetworkCredential _credentials;
		private readonly IConfiguration _configuration;
		private readonly ILoggerService _loggerService;
        private readonly IMediator _mediator;
		public ContactController(ApplicationDbContext context, IMediator mediator, IOptions<AppSettings> appSettings, IConfiguration configuration, ILoggerService loggerService)
		{
			var emailSettings = appSettings.Value.EmailSettings;
			_context = context;
			_credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
			_configuration = configuration;
			_loggerService = loggerService;
            _mediator = mediator;
		}
        [HttpPost]
        [Route("contact/send-email")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(ContactFormDto model, string viewName, string honeypotSpamContact, string gRecaptchaResponseContact)
        {
            SendEmailCommand command = new SendEmailCommand
            {
                Model = model,
                ViewName = viewName,
                HoneypotSpamContact = honeypotSpamContact,
                GRecaptchaResponseContact = gRecaptchaResponseContact
            };

            var result = await _mediator.Send(command);

            TempData["IsFormSubmitted"] = true;
            TempData["IsFormSuccess"] = result.IsSuccess;
            TempData["Message"] = result.Message;

            return View($"~/Views/{result.ViewName}.cshtml", result.Model);
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
	}
}