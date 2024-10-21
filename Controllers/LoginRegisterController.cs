using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using _200SXContact.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using _200SXContact.Data;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Diagnostics;

namespace _200SXContact.Controllers
{
	public class LoginRegisterController : Controller
	{
		private readonly SignInManager<User> _signInManager;
		private readonly ApplicationDbContext _context;
		private readonly IPasswordHasher<User> _passwordHasher;
		private readonly ILogger<LoginRegisterController> _logger;
		private readonly UserManager<User> _userManager;

		public LoginRegisterController(ApplicationDbContext context, ILogger<LoginRegisterController> logger, SignInManager<User> signInManager, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
			_passwordHasher = new PasswordHasher<User>();
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View("~/Views/Account/Login.cshtml");
		}
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{				
			if (!ModelState.IsValid)
			{
				return View("~/Views/Account/Login.cshtml", model);
			}

			var user = await _userManager.FindByNameAsync(model.Username);
			if (user != null)
			{
				if (!user.IsEmailVerified)
				{
					ModelState.AddModelError("", "Please verify your email before logging in.");
					TempData["Message"] = "User is not verified ! Check email.";
					TempData["IsUserVerified"] = "no";
					return View("~/Views/Account/Login.cshtml", model);
				}

				var passwordVerificationResult = await _userManager.CheckPasswordAsync(user, model.Password);
				if (passwordVerificationResult)
				{
					var claims = new List<Claim> 
					{ 
						new Claim(ClaimTypes.Name, user.UserName),
						new Claim(ClaimTypes.Email, user.Email),
						new Claim(ClaimTypes.NameIdentifier, user.Id)

					};
					var roles = await _userManager.GetRolesAsync(user);
					foreach (var role in roles)
					{
						claims.Add(new Claim(ClaimTypes.Role, role));
					}
					var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
					var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

					if (result.Succeeded)
					{
						Console.WriteLine("User logged in successfully.");
						return RedirectToAction("Dashboard", "Dashboard");
					}
					return RedirectToAction("Dashboard", "Dashboard");
				}
			}
			ModelState.AddModelError("","Invalid username or password.");
			TempData["Message"] = "Invalid username or password !";
			TempData["IsUserVerified"] = "no";
			return View("~/Views/Account/Login.cshtml", model);
		}
		[HttpGet]
		public IActionResult ForgotPassReset()
		{
			var model = new ForgotPasswordViewModel();
			if (model == null)
			{
				model = new ForgotPasswordViewModel();
			}
			return View("~/Views/Account/ForgotPassReset.cshtml", model);
		}
		[HttpGet]
		[AllowAnonymous]
		public IActionResult LoginMicrosoft()
		{
			var redirectUri = Url.Action("SigninMicrosoft", "LoginRegister", null, Request.Scheme);
			var properties = new AuthenticationProperties { RedirectUri = redirectUri };
			_logger.LogInformation("Initiating Microsoft login, redirecting to: " + redirectUri);
			return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> SigninMicrosoft()
		{
			var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			if (!result.Succeeded)
			{
				_logger.LogWarning("Authentication failed. Redirecting to login.");
				return RedirectToAction("Login", "LoginRegister");
			}

			var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value; 
			var username = result.Principal.FindFirst(ClaimTypes.Name)?.Value; 
			var userId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				user = new User
				{
					UserName = username,
					Email = email,
					CreatedAt = DateTime.UtcNow,
					IsEmailVerified = true, // Automatically verify email for OAuth users
					EmailVerificationToken = Guid.NewGuid().ToString() + "_MS_OAuth"
				};

				// Add user to the database
				var createResult = await _userManager.CreateAsync(user);
				if (!createResult.Succeeded)
				{
					// Log the errors
					foreach (var error in createResult.Errors)
					{
						_logger.LogError($"Error creating user: {error.Description}");
					}
					return RedirectToAction("Login", "LoginRegister"); // Redirect to login on error
				}
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email)
			};

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
			TempData["IsUserLoggedIn"] = true;
			ViewData["IsUserLoggedIn"] = true;
			ViewData["MessageLoginMicrosoft"] = "Logged in successfully with Microsoft !";
			return RedirectToAction("Dashboard", "Dashboard"); 
		}
		[HttpGet]
		public IActionResult Register()
		{
			return View("~/Views/Account/Register.cshtml", new RegisterViewModel()); // Return a new RegisterViewModel for the GET request
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Dashboard", "Account");
			}

			if (ModelState.IsValid)
			{
				// Check if the user already exists using UserManager
				var existingUser = await _userManager.FindByEmailAsync(model.Email)
								   ?? await _userManager.FindByNameAsync(model.Username);

				if (existingUser != null)
				{
					TempData["UserExists"] = "yes";
					TempData["Message"] = "User already exists!";
					return View("~/Views/Account/Register.cshtml", model);
				}

				var user = new User
				{
					UserName = model.Username,
					Email = model.Email,
					CreatedAt = DateTime.UtcNow,
					IsEmailVerified = false,
					EmailVerificationToken = Guid.NewGuid().ToString()
				};

				// Create user with UserManager, which handles password hashing
				var createResult = await _userManager.CreateAsync(user, model.Password);
				if (!createResult.Succeeded)
				{
					// Add errors to ModelState if user creation failed
					foreach (var error in createResult.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
					return View("~/Views/Account/Register.cshtml", model);
				}

				// Send verification email
				var verificationUrl = Url.Action("VerifyEmail", "LoginRegister", new { token = user.EmailVerificationToken }, Request.Scheme);
				await SendVerificationEmail(model.Email, verificationUrl);
				TempData["IsFormRegisterSuccess"] = "yes";
				TempData["Message"] = "Registration successful! Check your email.";

				return RedirectToAction("Login", "LoginRegister");
			}

			return View("~/Views/Account/Register.cshtml", model);
		}

		[HttpGet]
		public IActionResult ResetPassword(string token, string email)
		{
			// Ensure that both token and email are passed correctly
			if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
			{
				return BadRequest("Invalid password reset token or email.");
			}

			// Prepare the view model to pass to the view
			var model = new ResetPasswordViewModel
			{
				Token = token,
				Email = email
			};

			// Return the correct view with the model
			return View("~/Views/Account/ResetPass.cshtml", model);
		}
		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("~/Views/Account/ResetPass.cshtml", model); // Return the view with errors
			}

			// Find the user by email
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null || user.PasswordResetToken != model.Token)
			{
				ModelState.AddModelError("", "Invalid token or email.");
				return View("~/Views/Account/ResetPass.cshtml", model);
			}

			// Update the user's password
			var resetResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
			if (!resetResult.Succeeded)
			{
				foreach (var error in resetResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return View("~/Views/Account/ResetPass.cshtml", model);
			}

			// Clear the password reset token if needed
			user.PasswordResetToken = null; // Clear the token after successful reset
			await _userManager.UpdateAsync(user);

			TempData["PassResetSuccess"] = "yes";
			TempData["Message"] = "Your password has been reset successfully.";
			return RedirectToAction("Login", "LoginRegister");
		}

		private async Task SendPasswordResetEmail(string email, string resetUrl)
		{
			var fromAddress = new MailAddress("test@200sxproject.com", "Admin");
			var toAddress = new MailAddress(email);
			string subject = "200SX Project || Reset Your Password";
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
                <img src='https://200sxproject.com/images/verifHeader.JPG' alt='Header Image' />
            </div>
            <h1>Account Recovery</h1>
            <p>Hi there,</p>
            <p>Click the link below to reset and assign a new password for <b>MaintenApp</b>:</p>
            <p>
                <a href='" + resetUrl + @"' class='button'>Recover Your Account</a>
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
				Credentials = new System.Net.NetworkCredential("test@200sxproject.com", "Recall1547!")
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
				}
			}
		}
		[HttpPost]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("~/Views/Account/ForgotPassReset.cshtml", model); // Return the model with validation errors
			}

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				TempData["Message"] = "If that email is associated with an account, you will receive a password reset link.";
				return RedirectToAction("ForgotPassword");
			}

			// Generate a password reset token
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);

			// Create the reset URL
			var resetUrl = Url.Action("ResetPassword", "LoginRegister", new { token = token, email = user.Email }, Request.Scheme);

			// Send the password reset email
			await SendPasswordResetEmail(user.Email, resetUrl);
			TempData["PassResetLinkEmailed"] = "yes";
			TempData["Message"] = "Password reset link emailed!";
			return RedirectToAction("Login");
		}
		private async Task SendVerificationEmail(string email, string verificationUrl)
		{
			var fromAddress = new MailAddress("test@200sxproject.com", "Admin");
			var toAddress = new MailAddress(email);

			string subject = "200SX Project || Verify your email";

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
                <img src='https://200sxproject.com/images/verifHeader.JPG' alt='Header Image' />
            </div>
            <h1>Account Activation</h1>
            <p>Hi there,</p>
            <p>Thank you for registering your account for <b>MaintenApp</b> ! To activate your account, please click the button below:</p>
            <p>
                <a href='" + verificationUrl + @"' class='button'>Activate Your Account</a>
            </p>
            <p>If you did not sign up for this account, you can safely ignore this email.</p>
            <p>Thank you !</p>
            <div class='footer'>
                <p>© 2024 200SX Project. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>";
			//string body = "test";
			using (var smtpClient = new SmtpClient
			{
				Host = "mail5019.site4now.net",
				Port = 587,
				EnableSsl = true,
				Credentials = new System.Net.NetworkCredential("test@200sxproject.com", "Recall1547!")
			})
			{
				using (var message = new MailMessage(fromAddress, toAddress)
				{
					Subject = subject,
					Body = body,
					IsBodyHtml = true // Set this to true to send HTML content
				})
				{
					await smtpClient.SendMailAsync(message);
				}
			}
		}

		[HttpGet]
		public async Task<IActionResult> VerifyEmail(string token)
		{
			var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

			if (user == null)
			{
				return NotFound("Invalid token.");
			}

			user.IsEmailVerified = true;
			user.EmailVerificationToken = null;

			// No need to update explicitly, as saving changes will reflect the updates
			await _userManager.UpdateAsync(user);

			TempData["IsEmailVerifiedLogin"] = "yes";
			TempData["Message"] = "Email verified! You can now log in.";
			return RedirectToAction("Login", "LoginRegister");
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}

}



