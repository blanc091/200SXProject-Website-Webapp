using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _200SXContact.Queries.Areas.Newsletter;
using MediatR;
using _200SXContact.Commands.Areas.Newsletter;
using _200SXContact.Models.Areas.Newsletter;
using _200SXContact.Models.DTOs.Areas.Newsletter;
using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Helpers;

namespace _200SXContact.Controllers.Areas.Admin
{
    [Route("newsletter")]
    public class NewsletterController : Controller
	{
		private readonly ILoggerService _loggerService;
		private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public NewsletterController(ApplicationDbContext context, IConfiguration configuration, ILoggerService loggerService, IMediator mediator)
        {
            _context = context;
            _configuration = configuration;
            _loggerService = loggerService;
            _mediator = mediator;
        }
        [HttpGet]
		[Route("create-newsletter-admin")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateNewsletter()
		{
            NewsletterDto? model = await _mediator.Send(new GetCreateNewsletterViewQuery());
            if (model is null)
			{
                await _loggerService.LogAsync("Newsletter || Could not get admin newsletter view", "Error", "");

                return BadRequest("Could not get admin newsletter view.");
			}

            return View("~/Views/Newsletter/CreateNewsletter.cshtml", model);			
		}        
		[HttpPost]
        [Route("subscribe")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string email, string honeypotSpam, string gRecaptchaResponseNewsletter)
        {
            IActionResult? result = await _mediator.Send(new SubscribeToNewsletterCommand
            {
                Email = email,
                HoneypotSpam = honeypotSpam,
                RecaptchaResponse = gRecaptchaResponseNewsletter
            });

            if (result is RedirectToActionResult redirectResult)
            {
                if (redirectResult.RouteValues != null)
                {
                    if (redirectResult.RouteValues.TryGetValue("Message", out var message))
                    {
                        TempData["Message"] = message;                        
                    }

                    if (redirectResult.RouteValues.TryGetValue("IsNewsletterSubscribed", out var isSubscribed))
                    {
                        TempData["IsNewsletterSubscribed"] = isSubscribed;
                    }

                    if (redirectResult.RouteValues.TryGetValue("IsNewsletterError", out var isError))
                    {
                        TempData["IsNewsletterError"] = isError;
                    }
                }

                return View("~/Views/Home/Index.cshtml");
            }

            if (result is BadRequestObjectResult badRequestResult)
            {
                await _loggerService.LogAsync("Could not subscribe to the newsletter " + badRequestResult.Value?.ToString(), "Error", "");

                TempData["Message"] = badRequestResult.Value?.ToString();
                TempData["IsNewsletterSubscribed"] = "no";
                TempData["IsNewsletterError"] = "yes";

                return View("~/Views/Home/Index.cshtml");
            }

            if (result is null)
            {
                await _loggerService.LogAsync("Could not subscribe to the newsletter", "Error", "");

                TempData["IsNewsletterSubscribed"] = "no";
                TempData["IsNewsletterError"] = "yes";
                TempData["Message"] = "Failed to subscribe to newsletter !";

                return BadRequest("Could not subscribe to newsletter.");
            }

            return View("~/Views/Home/Index.cshtml");
        }
        [HttpGet]
        [Route("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                await _loggerService.LogAsync("Newsletter || Email not provided for unsubscribe request", "Error", "");

                return BadRequest("Email is required.");
            }

            try
            {
                await _mediator.Send(new UnsubscribeFromNewsletterCommand { Email = email });
                TempData["Unsubscribed"] = "yes";
                TempData["Message"] = "Unsubscribed from the newsletter !";

                await _loggerService.LogAsync("Newsletter || Unsubscribed successfully for " + email, "Info", "");

                return Redirect("/");
            }
            catch (ArgumentException ex)
            {
                await _loggerService.LogAsync("Newsletter || Error: " + ex.Message, "Error", "");

                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
				await _loggerService.LogAsync("Newsletter || Error: " + ex.Message, "Error", "");

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync("Newsletter || Unexpected error: " + ex.Message, "Error", "");

                return StatusCode(500, "Internal server error");
            }
        }
		[HttpPost]
		[Route("send-newsletter-admin")]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNewsletter(NewsletterViewModel model)
        {
            await _loggerService.LogAsync("Newsletter || Started sending newsletter admin", "Info", "");

            if (!ModelState.IsValid)
            {
                await _loggerService.LogAsync("Newsletter || Invalid model state when sending newsletter admin", "Error", "");

                var sanitizedModel = new NewsletterViewModel
                {
                    Subject = model.Subject,
                    Body = HtmlSanitizerHelper.Sanitize(model.Body) 
                };

                return View("~/Views/Newsletter/CreateNewsletter.cshtml", sanitizedModel);
            }

            await _mediator.Send(new SendNewsletterCommand { Subject = model.Subject, Body = model.Body });
            TempData["Message"] = "Newsletter sent successfully !";

            return RedirectToAction("CreateNewsletter", "Newsletter");
        }
    }
}
