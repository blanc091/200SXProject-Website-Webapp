using _200SXContact.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

namespace _200SXContact.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
		[Route("")]
		[Route("home/index")]
		public IActionResult Index(ContactForm model = null)
		{
			return View(model); 
		}
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
		[HttpGet]
		[Route("api/is-logged-in")]
		public IActionResult IsLoggedIn()
		{
			return Json(User.Identity.IsAuthenticated);
		}
	}
}
