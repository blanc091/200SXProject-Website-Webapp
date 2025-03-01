using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Models.Configs;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace _200SXContact.Controllers.Account
{
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly SignInManager<User> _signInManager;
		private readonly ILoggerService _loggerService;
		private readonly NetworkCredential _credentials;
		public AccountController(IOptions<AppSettings> appSettings, ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, ILoggerService loggerService)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
			_loggerService = loggerService;
			var emailSettings = appSettings.Value.EmailSettings;
			_credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
		}
		[HttpGet]
		[Route("account/admin-dashboard")]
		[Authorize(Roles = "Admin")]
		public IActionResult AdminDash()
		{
			_loggerService.LogAsync("Account || Getting admin dash page", "Info", "");
			return View("~/Views/Account/AdminDash.cshtml");
		}
		[HttpGet]
		[Route("account/login-account")]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl = null)
		{
			await _loggerService.LogAsync("Account || Getting login page", "Info", "");
			if (!User.Identity.IsAuthenticated)
			{
				TempData["isNiceTry"] = "yes";
				TempData["Message"] = "Nice try :)";
				ViewData["ReturnUrl"] = returnUrl;
				await _loggerService.LogAsync("Account || Returned nice try from login page", "Info", "");
				return View("~/Views/Newsletter/AccessDenied.cshtml");
			}
			else
			{
				await _loggerService.LogAsync("Account || Getting user in login page", "Info", "");
				var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
				var user = await userManager.GetUserAsync(User);
				if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
				{
					return View("~/Views/Newsletter/CreateNewsletter.cshtml");
				}
				else
				{
					await _loggerService.LogAsync("Account || Returned nice try from login page for user not found", "Info", "");
					TempData["isNiceTry"] = "yes";
					TempData["Message"] = "Nice try :)";
					ViewData["ReturnUrl"] = returnUrl;
					await _loggerService.LogAsync("Account || User logged in", "Info", "");
					return View("~/Views/Home/Index.cshtml");
				}
			}
		}
		[HttpGet]
		[Route("account/access-denied-account")]
		[AllowAnonymous]
		public async Task<IActionResult> AccessDenied(string returnUrl = null)
		{
			await _loggerService.LogAsync("Account || Getting access denied page", "Info", "");
			if (!User.Identity.IsAuthenticated)
			{
				ViewData["ReturnUrl"] = returnUrl;
				await _loggerService.LogAsync("Account || User not authenticated in access denied page", "Error", "");
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
				await _loggerService.LogAsync("Account || Got access denied page", "Info", "");
				return View("~/Views/Home/Index.cshtml");
			}
		}
		[HttpGet]
		[Route("account/user-profile")]
		[Authorize]
		public async Task<IActionResult> UserProfile()
		{
			await _loggerService.LogAsync("Account || Getting user profile page", "Info", "");
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userBuilds = await _context.UserBuilds
				.Where(b => b.UserId == userId)
				.OrderByDescending(b => b.DateCreated)
				.ToListAsync();
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				await _loggerService.LogAsync("Account || User is null in profile page", "Error", "");
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
			await _loggerService.LogAsync("Account || Got user profile page", "Info", "");
			return View("~/Views/Account/UserDash.cshtml", viewModel);
		}
		[HttpGet]
		[Route("account/create-test-user")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateTestUser(string userName = "testAccount", string password = "Test@cc34")
		{
			await _loggerService.LogAsync($"Account || Attempting to create test user: {userName}", "Info", "");

			var testUser = new User
			{
				UserName = userName,
				Email = $"{userName}@example.com",
				EmailConfirmed = true,
				LockoutEnabled = true,
				CreatedAt = DateTime.UtcNow,
				LastLogin = null,
				IsEmailVerified = true
			};

			var userExists = await _userManager.FindByNameAsync(userName);
			if (userExists == null)
			{
				var result = await _userManager.CreateAsync(testUser, password);
				if (result.Succeeded)
				{
					await _loggerService.LogAsync($"Account || Test user '{userName}' created successfully", "Info", "");
					return RedirectToAction("Index", "Home");
				}
				else
				{
					await _loggerService.LogAsync($"Account || Failed to create test user '{userName}'", "Error", "");
					foreach (var error in result.Errors)
					{
						await _loggerService.LogAsync($"Account || {error.Description}", "Error", "");
					}
					return RedirectToAction("Error", "Home");
				}
			}
			else
			{
				await _loggerService.LogAsync($"Account || Test user '{userName}' already exists", "Error", "");
				return RedirectToAction("Error", "Home");
			}
		}
		//[HttpGet]
		[Route("account/delete-account-confirmation")]
		public IActionResult DeleteAccountConfirmation()
		{
			return View("~/Views/Account/DeleteUser.cshtml");
		}
		[HttpPost]
		[Route("account/delete-account-verify")]
		public async Task<IActionResult> DeleteAccountVerify(string userEmail)
		{
			var userExists = await _userManager.FindByEmailAsync(userEmail);
			if (userExists == null)
			{
				TempData["Message"] = "User does not exist!";
				return RedirectToAction("DeleteAccountConfirmation", "Account");
			}
			var token = await _userManager.GenerateUserTokenAsync(userExists,
								"Default", "DeleteAccountToken");
			var resetUrl = Url.Action("DeleteAccount", "Account",
								new { email = userEmail, token }, Request.Scheme);
			await SendUserDeleteEmail(userEmail, resetUrl);
			TempData["Message"] = "Account deletion email sent.";
			TempData["UserDeleteAccEmailSent"] = "yes";
			return RedirectToAction("DeleteAccountConfirmation", "Account");
		}
		private async Task SendUserDeleteEmail(string email, string resetUrl)
		{
			await _loggerService.LogAsync("Account || Starting sending user delete email task", "Info", "");
			var fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
			var toAddress = new MailAddress(email);
			string subject = "Import Garage || Delete Your Account";
			string body = @"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>
            body {
                font-family: 'Helvetica', 'Roboto', sans-serif;
                margin: 0;
                padding: 0;
                background-color: #2c2c2c; 
                color: #ffffff; 
            }
            .container {
                width: 100%;
                max-width: 600px;
                margin: 0 auto;
                padding: 20px;
                background-color: #3c3c3c;
                border-radius: 8px;
                box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
            }
            .header {
                text-align: center;
            }
            .header img {
                max-width: 100%;
                height: auto;
                border-radius: 8px;
            }
            h1 {
                color: #f5f5f5;
                font-size: 24px;
				font-family: 'Helvetica', 'Roboto', sans-serif;
				margin: 20px 0;
            }
            p {
                line-height: 1.6;
                margin: 10px 0;
				color: #f5f5f5;
				font-family: 'Helvetica', 'Roboto', sans-serif;
            }
            .button {
                display: inline-block;
                padding: 10px 20px;
                font-size: 16px;
                font-weight: bold;
                color: #ffffff;
                background-color: #d0bed1;
                text-decoration: none;
                border-radius: 5px;
                transition: background-color 0.3s ease;
            }
            .button:hover {
                background-color: #966b91; 
            }
            .footer {
                text-align: center;
                margin-top: 20px;
                font-size: 12px;
                color: #b0b0b0; 
            }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <a href=""https://www.200sxproject.com"" target=""_blank"">
					<img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
				</a>
            </div>
            <h1>Account Deletion</h1>
            <p>Hi there,</p>
            <p>Click the link below to delete your account and all related info; this action cannot be undone:</p>
            <p>
                <a href='" + resetUrl + @"' class='button'>Delete Your Account</a>
            </p>
            <p>If you did not request this, you can safely ignore this email.</p>
            <p>Thank you !</p>
            <div class='footer'>
                <p>© 2024 200SX Project. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>";

			using (var smtpClient = new SmtpClient
			{
				Host = "mail5019.site4now.net",
				Port = 587,
				EnableSsl = true,
				Credentials = new NetworkCredential(_credentials.UserName, _credentials.Password)
			})
			{
				using (var message = new MailMessage(fromAddress, toAddress)
				{
					Subject = subject,
					Body = body,
					IsBodyHtml = true
				})
				{
					await smtpClient.SendMailAsync(message);
					await _loggerService.LogAsync("Account || Finished sending user delete email task", "Info", "");
				}
			}
		}
		//[HttpPost]
		[Route("account/delete-account")]
		//[Authorize]
		public async Task<IActionResult> DeleteAccount(string email, string token)
		{
            await _loggerService.LogAsync("Account || Deleting user in user profile page", "Info", "");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null || !await _userManager.VerifyUserTokenAsync(user, "Default", "DeleteAccountToken", token))
			{
                await _loggerService.LogAsync("Account || User not found in Deleting account method, or the email token is invalid", "Error", "");
                return NotFound("User not found.");
			}
			var userBuilds = _context.UserBuilds.Where(ub => ub.UserId == userId).ToList();
			if (userBuilds.Any())
			{
                await _loggerService.LogAsync("Account || User build found, " + userBuilds.ToString() +  "deleting..", "Info", "");
                _context.UserBuilds.RemoveRange(userBuilds);
				await _context.SaveChangesAsync();
                await _loggerService.LogAsync("Account || Deleted user build", "Info", "");
            }
			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
			{                
                await _signInManager.SignOutAsync();
                await _loggerService.LogAsync("Account || Deleting/signing out user after deleted builds", "Info", "");
				TempData["Message"] = "Account deletion completed successfully.";
				TempData["UserDeletedAcc"] = "yes";
				return RedirectToAction("Index", "Home"); 
			}
			ModelState.AddModelError(string.Empty, "An error occurred while deleting your account.");
            await _loggerService.LogAsync("Account || Finished deleting user at user profile page", "Info", "");
            return RedirectToAction("UserProfile");
		}
	}
}
