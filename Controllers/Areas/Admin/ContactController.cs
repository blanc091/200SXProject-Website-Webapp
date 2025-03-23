using _200SXContact.Data;
using _200SXContact.Models.Configs;
using _200SXContact.Commands.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Helpers.Areas.Admin;

namespace _200SXContact.Controllers.Areas.Admin
{
    public class ContactController : Controller
	{
		private readonly ILoggerService _loggerService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
		public ContactController(ApplicationDbContext context, IMapper mapper, IMediator mediator, IOptions<AppSettings> appSettings, IConfiguration configuration, ILoggerService loggerService)
		{
			var emailSettings = appSettings.Value.EmailSettings;
			_loggerService = loggerService;
            _mediator = mediator;
            _mapper = mapper;
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

            if (!result.IsSuccess)
            {
                TempData["IsFormSubmitted"] = false;
                TempData["IsFormSuccess"] = result.IsSuccess;
                TempData["Message"] = result.Message;
                TempData["ContactError"] = "yes";

                return View($"~/Views/{result.ViewName}.cshtml", model);
            }

            ContactFormDto contactFormDto = _mapper.Map<ContactFormDto>(result.Model);
            await _loggerService.LogEmailAsync(model, "Sent");
            TempData["IsFormSubmitted"] = true;
            TempData["IsFormSuccess"] = result.IsSuccess;
            TempData["Message"] = result.Message;

            await _loggerService.LogAsync("Contact form || Email sent to admin", "Info", "");

            return View($"~/Views/{result.ViewName}.cshtml", contactFormDto);
        }	
	}
}