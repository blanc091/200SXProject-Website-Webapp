using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
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
		private readonly ILoggerService _loggerService;
		public AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, ILoggerService loggerService)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
			_loggerService = loggerService;
		}
		[HttpGet]
		[Route("account/admin-dashboard")]
		[Authorize(Roles = "Admin")]
		public IActionResult AdminDash()
		{
            _loggerService.LogAsync("Getting admin dash page", "Info", "");
            return View("~/Views/Account/AdminDash.cshtml");
		}		
		[HttpGet]
		[Route("account/login-account")]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl = null)
		{
            await _loggerService.LogAsync("Getting login page", "Info", "");
            if (!User.Identity.IsAuthenticated)
			{
                TempData["isNiceTry"] = "yes";
                TempData["Message"] = "Nice try :)";
                ViewData["ReturnUrl"] = returnUrl;
                await _loggerService.LogAsync("Returned nice try from login page", "Info", "");
                return View("~/Views/Newsletter/AccessDenied.cshtml");				
			}
			else
			{
                await _loggerService.LogAsync("Getting user in login page", "Info", "");
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
				var user = await userManager.GetUserAsync(User);
				if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
				{
					return View("~/Views/Newsletter/CreateNewsletter.cshtml");
				}
				else
				{
                    await _loggerService.LogAsync("Returned nice try from login page for user not found", "Info", "");
                    TempData["isNiceTry"] = "yes";
					TempData["Message"] = "Nice try :)";
					ViewData["ReturnUrl"] = returnUrl;
                    await _loggerService.LogAsync("User logged in", "Info", "");
                    return View("~/Views/Home/Index.cshtml");
				}               
            }
        }
		[HttpGet]
		[Route("account/access-denied-account")]
		[AllowAnonymous]
		public async Task<IActionResult> AccessDenied(string returnUrl = null)
		{
            await _loggerService.LogAsync("Getting access denied page", "Info", "");
            if (!User.Identity.IsAuthenticated)
			{
				ViewData["ReturnUrl"] = returnUrl;
                await _loggerService.LogAsync("user not authenticated in access denied page", "Error", "");
                return View("~/Views/Newsletter/AccessDenied.cshtml");
			}
			else
			{
				var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
				var user = await userManager.GetUserAsync(User);
				if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
				{
                    await _loggerService.LogAsync("User is null or not in admin role in AccessDenied page", "Error", "");
                    return View("~/Views/Newsletter/CreateNewsletter.cshtml");
				}
				ViewData["ReturnUrl"] = returnUrl;
                await _loggerService.LogAsync("Got access denied page", "Info", "");
                return View("~/Views/Home/Index.cshtml");
			}
		}
		[HttpGet]
		[Route("account/user-profile")]
		[Authorize]
		public async Task<IActionResult> UserProfile()
		{
            await _loggerService.LogAsync("Getting user profile page", "Info", "");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userBuilds = await _context.UserBuilds
				.Where(b => b.UserId == userId)
				.OrderByDescending(b => b.DateCreated)
				.ToListAsync();
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
                await _loggerService.LogAsync("User is null in profile page", "Error", "");
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
            await _loggerService.LogAsync("Got user profile page", "Info", "");
            return View("~/Views/Account/UserDash.cshtml", viewModel);
		}
		[HttpPost]
		[Route("account/delete-account")]
		[Authorize]
		public async Task<IActionResult> DeleteAccount()
		{
            await _loggerService.LogAsync("Deleting user in user profile page", "Info", "");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
                await _loggerService.LogAsync("User not found in Deleting account method", "Error", "");
                return NotFound("User not found.");
			}
			var userBuilds = _context.UserBuilds.Where(ub => ub.UserId == userId).ToList();
			if (userBuilds.Any())
			{
                await _loggerService.LogAsync("User build found, " + userBuilds.ToString() +  "deleting..", "Info", "");
                _context.UserBuilds.RemoveRange(userBuilds);
				await _context.SaveChangesAsync();
                await _loggerService.LogAsync("Deleted user build", "Info", "");
            }
			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
			{                
                await _signInManager.SignOutAsync();
                await _loggerService.LogAsync("Deleting/signing out user after deleted builds", "Info", "");
                return RedirectToAction("Index", "Home"); 
			}
			ModelState.AddModelError(string.Empty, "An error occurred while deleting your account.");
            await _loggerService.LogAsync("Finished deleting user at user profile page", "Info", "");
            return RedirectToAction("UserProfile");
		}
	}
}
