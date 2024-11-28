using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace _200SXContact.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly SignInManager<User> _signInManager;
		public AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
		}
		[HttpGet]
		[Route("account/admin-dashboard")]
		[Authorize(Roles = "Admin")]
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
					ViewData["ReturnUrl"] = returnUrl;
					return View("~/Views/Home/Index.cshtml");
				}

			}
		}
		[HttpGet]
		[Route("account/user-profile")]
		[Authorize]
		public async Task<IActionResult> UserProfile()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userBuilds = await _context.UserBuilds
				.Where(b => b.UserId == userId)
				.OrderByDescending(b => b.DateCreated)
				.ToListAsync();

			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return NotFound("User not found");
			}

			var viewModel = new UserProfileViewModel
			{
				UserName = user.UserName,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				LastLogin = user.LastLogin,
				UserBuilds = userBuilds
			};

			return View("~/Views/Account/UserDash.cshtml", viewModel);
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> DeleteAccount()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}
			var userBuilds = _context.UserBuilds.Where(ub => ub.UserId == userId).ToList();
			if (userBuilds.Any())
			{
				_context.UserBuilds.RemoveRange(userBuilds);
				await _context.SaveChangesAsync();
			}
			var result = await _userManager.DeleteAsync(user);

			if (result.Succeeded)
			{
				await _signInManager.SignOutAsync();
				return RedirectToAction("Index", "Home"); 
			}
			ModelState.AddModelError(string.Empty, "An error occurred while deleting your account.");
			return RedirectToAction("UserProfile");
		}

	}
}
