using _200SXContact.Data;
using _200SXContact.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Climate;

namespace _200SXContact.Controllers
{
	public class DetailedViewsController : Controller
	{
		private readonly ILoggerService _loggerService;
		public DetailedViewsController(ILoggerService loggerService) 
		{
			_loggerService = loggerService;
        }
		[HttpGet]
		[Route("detailed-view/{id}")]
		public IActionResult DetailedView(string id)
		{
            _loggerService.LogAsync("Home || Starting getting detailed index view", "Info", "");
            if (!string.IsNullOrEmpty(id))
			{
				var sanitizedId = id.Replace(" ", "-");
                _loggerService.LogAsync("Home || Got detailed index view", "Info", "");
                return View($"~/Views/DetailedViews/{sanitizedId}.cshtml");
			}
			else
			{
                _loggerService.LogAsync("Home || ID is empty when getting detailed index view", "Error", "");
                return RedirectToAction("Index", "Home"); 
			}
		}
	}
}
