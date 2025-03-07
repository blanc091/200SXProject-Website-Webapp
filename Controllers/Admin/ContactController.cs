using Microsoft.AspNetCore.Mvc;
using _200SXContact.Models;
using System.Net.Mail;
using _200SXContact.Data;
using _200SXContact.Models.Configs;
using Microsoft.Extensions.Options;
using System.Net;
using MediatR;
using _200SXContact.Commands.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Helpers.Areas.Admin;

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
            await _loggerService.LogAsync("Contact form || Starting sending email to admin", "Info", "");

            SendEmailCommand command = new SendEmailCommand
            {
                Model = model,
                ViewName = viewName,
                HoneypotSpamContact = honeypotSpamContact,
                GRecaptchaResponseContact = gRecaptchaResponseContact
            };

            ContactResult result = await _mediator.Send(command);
            await _loggerService.LogEmailAsync(model, "Sent");
            TempData["IsFormSubmitted"] = true;
            TempData["IsFormSuccess"] = result.IsSuccess;
            TempData["Message"] = result.Message;

            await _loggerService.LogAsync("Contact form || Email sent to admin", "Info", "");

            return View($"~/Views/{result.ViewName}.cshtml", result.Model);
        }	
	}
}