using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _200SXContact.Controllers
{
	public class AccountController : Controller
	{
		[HttpGet]
		[AllowAnonymous]
		public IActionResult AdminDash()
		{
			return View("~/Views/Account/AdminDash.cshtml");
		}
			[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl = null)
		{
			if (!User.Identity.IsAuthenticated)
			{
                TempData["isNiceTry"] = "yes";
                TempData["Message"] = "Nice try :)";
                ViewData["ReturnUrl"] = returnUrl;
				return View("~/Views/Newsletter/AccessDenied.cshtml");				
			}
			else
			{
				{
					var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
					var user = await userManager.GetUserAsync(User);

					if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
					{
						return View("~/Views/Newsletter/CreateNewsletter.cshtml");
					}
					else
					{
						TempData["isNiceTry"] = "yes";
						TempData["Message"] = "Nice try :)";
						ViewData["ReturnUrl"] = returnUrl;
						return View("~/Views/Home/Index.cshtml");
					}
				}

			}
		}
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> AccessDenied(string returnUrl = null)
		{
			if (!User.Identity.IsAuthenticated)
			{
				ViewData["ReturnUrl"] = returnUrl;
				return View("~/Views/Newsletter/AccessDenied.cshtml");
			}
			else
			{
				{
					var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
					var user = await userManager.GetUserAsync(User);

					if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
					{
						return View("~/Views/Newsletter/CreateNewsletter.cshtml");
					}
					//implement nice try modal here
					ViewData["ReturnUrl"] = returnUrl;
					return View("~/Views/Home/Index.cshtml");
				}

			}
		}
	}
}
