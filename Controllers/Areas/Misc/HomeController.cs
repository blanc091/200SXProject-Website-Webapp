using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models;
using _200SXContact.Models.DTOs.Areas.Admin;
using System.Diagnostics;

namespace _200SXContact.Controllers.Areas.Misc
{
    public class HomeController : Controller
    {
        private readonly ILoggerService _loggerService;
        public HomeController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        [HttpGet]
		[Route("home/policy")]
        public async Task<IActionResult> Policy()
        {
            await _loggerService.LogAsync("Home || Getting policy page", "Info", "");

            return View("~/Views/Home/PrivacyPolicy.cshtml"); 
		}
		[HttpGet]
		[Route("")]
		[Route("home/index")]
        public async Task<IActionResult> Index(ContactFormDto model = null)
		{
			await _loggerService.LogAsync("Home || Getting index page", "Info", "");

			return View(model);
		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            await _loggerService.LogAsync("Home || Return if error view", "Info", "");

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
		[HttpGet]
		[Route("api/is-logged-in")]
        public async Task<IActionResult> IsLoggedIn()
		{
            await _loggerService.LogAsync("Home || Getting IsLoggedIn JSON for api/is-logged-in", "Info", "");

            return Json(User.Identity.IsAuthenticated);
		}
	}
}
