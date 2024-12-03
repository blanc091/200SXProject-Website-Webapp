using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using _200SXContact.Models;
using _200SXContact.Models.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using _200SXContact.Data;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Extensions.Options;
using _200SXContact.Services;

namespace _200SXContact.Controllers
{
	public class LoginRegisterController : Controller
	{
		private readonly SignInManager<User> _signInManager;
		private readonly ApplicationDbContext _context;
		private readonly IPasswordHasher<User> _passwordHasher;
		private readonly ILoggerService _loggerService;
		private readonly UserManager<User> _userManager;
		private readonly NetworkCredential _credentials;
		private readonly IConfiguration _configuration;
		public LoginRegisterController(ApplicationDbContext context, IOptions<AppSettings> appSettings, ILoggerService loggerService, SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
			_passwordHasher = new PasswordHasher<User>();
			_loggerService = loggerService;
			var emailSettings = appSettings.Value.EmailSettings;
			_credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
			_configuration = configuration;
		}
		[HttpGet]
		[Route("login-with-microsoft")]
		[AllowAnonymous]
		public IActionResult LoginMicrosoft()
		{
			var redirectUri = Url.Action("SigninMicrosoft", "LoginRegister", null, Request.Scheme);
			var properties = new AuthenticationProperties { RedirectUri = redirectUri };
			_loggerService.LogAsync($"Initiating Microsoft login, redirecting to: {redirectUri}", "Info", "");
			return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
		}
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> SigninMicrosoft()
		{
			var result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);
			if (!result.Succeeded)
			{
				await _loggerService.LogAsync("Microsoft login failed.", "Error","");
				return RedirectToAction("Login");
			}
			var emailClaim = result.Principal.FindFirst(ClaimTypes.Email);
			var email = emailClaim?.Value;
			var username = email.Split('@')[0];
			username = Regex.Replace(username, @"[^a-zA-Z0-9]", string.Empty);
			if (string.IsNullOrEmpty(email))
			{
				await _loggerService.LogAsync("Email claim not found in the authentication result.", "Error", "");
				return RedirectToAction("Login");
			}
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				user = new User
				{
					UserName = username,
					Email = email,
					EmailConfirmed = true,
					CreatedAt = DateTime.UtcNow,
					IsEmailVerified = true
				};
				var createUserResult = await _userManager.CreateAsync(user);
				if (!createUserResult.Succeeded)
				{
					await _loggerService.LogAsync("Failed to create user: {Errors}" + string.Join(", ", createUserResult.Errors.Select(e => e.Description)), "Error","");
					return RedirectToAction("Login");
				}
			}
			user.LastLogin = DateTime.UtcNow;
			var updateResult = await _userManager.UpdateAsync(user);
			await _signInManager.SignInAsync(user, isPersistent: true);
			TempData["IsUserLoggedIn"] = true;
			ViewData["IsUserLoggedIn"] = true;
			TempData["MicrosoftLogin"] = true;
			ViewData["MessageLoginMicrosoft"] = "Logged in successfully with Microsoft !";
			return RedirectToAction("Dashboard", "Dashboard");
		}
		[HttpGet]
		[Route("login-page")]
		[AllowAnonymous]
		public IActionResult Login(string returnUrl = null)
		{
			if (returnUrl != null && !Url.IsLocalUrl(returnUrl))
			{
				returnUrl = null;
			}
			if (!User.Identity.IsAuthenticated)
			{
				ViewData["ReturnUrl"] = returnUrl;
				return View("~/Views/Account/Login.cshtml");
			}
			else
			{
				return RedirectToAction("Dashboard", "Dashboard");
			}
		}
		[HttpPost]
		[Route("login")]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			returnUrl = returnUrl ?? Url.Action("Dashboard", "Dashboard");
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
						user.LastLogin = DateTime.UtcNow;
						var updateResult = await _userManager.UpdateAsync(user);
						Console.WriteLine("User logged in successfully.");
						return LocalRedirect(returnUrl);
					}
				}
			}
			ModelState.AddModelError("", "Invalid username or password.");
			TempData["Message"] = "Invalid username or password !";
			TempData["IsUserVerified"] = "no";
			return View("~/Views/Account/Login.cshtml", model);
		}
		[HttpGet]
		[Route("forgot-my-password")]
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
		[Route("register-new-account")]
		public IActionResult Register()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return View("~/Views/Account/Register.cshtml", new RegisterViewModel());
			}
			else
			{
				return RedirectToAction("Dashboard", "Dashboard");
			}
		}
		[HttpPost]
		[Route("register-an-account")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model, string gRecaptchaResponseRegister)
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Dashboard", "Dashboard");
			}
			if (string.IsNullOrWhiteSpace(gRecaptchaResponseRegister) || !await VerifyRecaptchaAsync(gRecaptchaResponseRegister))
			{
				TempData["IsFormRegisterSuccess"] = "no";
				TempData["Message"] = "Failed reCAPTCHA validation.";
				return View("~/Views/Home/Index.cshtml");
			}
			ModelState.Remove(nameof(model.honeypotSpam));
			if (!ModelState.IsValid)
			{
				return View("~/Views/Account/Register.cshtml", model);
			}
			if (!string.IsNullOrWhiteSpace(model.honeypotSpam))
			{
				return BadRequest("Spam detected");
			}
			var existingUser = await _userManager.FindByEmailAsync(model.Email)
							   ?? await _userManager.FindByNameAsync(model.Username);
			if (existingUser != null)
			{
				TempData["UserExists"] = "yes";
				TempData["Message"] = "User already exists !";
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
			var createResult = await _userManager.CreateAsync(user, model.Password);
			if (!createResult.Succeeded)
			{
				foreach (var error in createResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return View("~/Views/Account/Register.cshtml", model);
			}
			if (model.SubscribeToNewsletter)
			{
				await Subscribe(model.Email);
			}
			var verificationUrl = Url.Action("VerifyEmail", "LoginRegister", new { token = user.EmailVerificationToken }, Request.Scheme);
			await SendVerificationEmail(model.Email, verificationUrl);
			TempData["IsFormRegisterSuccess"] = "yes";
			TempData["Message"] = "Registration successful! Check your email for verification.";
			return Redirect("/login-page");
		}
		private async Task<bool> VerifyRecaptchaAsync(string token)
		{
			var secretKey = _configuration["Recaptcha:SecretKey"];
			using (var client = new HttpClient())
			{
				var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "secret", secretKey },
					{ "response", token }
				});
				var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent);
				if (response.IsSuccessStatusCode)
				{
					var jsonString = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"reCAPTCHA Response: {jsonString}");

					dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
					return result.success == true;
				}
				else
				{
					Console.WriteLine($"Failed to verify reCAPTCHA: {response.StatusCode}");
				}
			}
			return false;
		}
		private async Task Subscribe(string email)
		{
			var existingSubscription = _context.NewsletterSubscriptions
				.FirstOrDefault(sub => sub.Email == email);
			if (existingSubscription == null)
			{
				var subscription = new NewsletterSubscription
				{
					Email = email,
					IsSubscribed = true,
					SubscribedAt = DateTime.Now
				};
				_context.NewsletterSubscriptions.Add(subscription);
			}
			else if (!existingSubscription.IsSubscribed)
			{
				existingSubscription.IsSubscribed = true;
				existingSubscription.SubscribedAt = DateTime.Now;
			}
			await _context.SaveChangesAsync();
		}
		[HttpGet]
		[Route("reset-my-password")]
		public IActionResult ResetPassword(string token, string email)
		{
			if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
			{
				return BadRequest("Invalid password reset token or email.");
			}
			var model = new ResetPasswordViewModel
			{
				Token = token,
				Email = email
			};
			return View("~/Views/Account/ResetPass.cshtml", model);
		}
		[HttpPost]
		[Route("forgot-password")]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("~/Views/Account/ForgotPassReset.cshtml", model);
			}
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				TempData["Message"] = "If that email is associated with an account, you will receive a password reset link.";
				return RedirectToAction("ForgotPassword");
			}
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var encodedToken = System.Web.HttpUtility.UrlEncode(token);
			var resetUrl = Url.Action("ResetPassword", "LoginRegister",
				new { token = encodedToken, email = user.Email }, Request.Scheme);
			await SendPasswordResetEmail(user.Email, resetUrl);
			TempData["PassResetLinkEmailed"] = "yes";
			TempData["Message"] = "Password reset link emailed !";
			return Redirect("/login-page");
		}
		[HttpPost]
		[Route("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("~/Views/Account/ResetPass.cshtml", model);
			}
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				ModelState.AddModelError("", "Invalid token or email.");
				return View("~/Views/Account/ResetPass.cshtml", model);
			}
			var decodedToken = System.Web.HttpUtility.UrlDecode(model.Token);
			var resetResult = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
			if (!resetResult.Succeeded)
			{
				foreach (var error in resetResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return View("~/Views/Account/ResetPass.cshtml", model);
			}
			TempData["PassResetSuccess"] = "yes";
			TempData["Message"] = "Your password has been reset successfully.";
			return Redirect("/login-page");
		}
		[HttpGet]
		[Route("access-denied")]
		public IActionResult AccessDenied()
		{
			return View("~/Views/Newsletter/AccessDenied.cshtml");
        }
		private async Task SendPasswordResetEmail(string email, string resetUrl)
		{
			var fromAddress = new MailAddress(_credentials.UserName, "Admin");
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
				Credentials = new System.Net.NetworkCredential(_credentials.UserName, _credentials.Password)
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
		private async Task SendVerificationEmail(string email, string verificationUrl)
		{
			var fromAddress = new MailAddress(_credentials.UserName, "Admin");
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
			using (var smtpClient = new SmtpClient
			{
				Host = "mail5019.site4now.net",
				Port = 587,
				EnableSsl = true,
				Credentials = new System.Net.NetworkCredential(_credentials.UserName, _credentials.Password)
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
			await _userManager.UpdateAsync(user);
			TempData["IsEmailVerifiedLogin"] = "yes";
			TempData["Message"] = "Email verified ! You can now log in.";
			return Redirect("/login-page");
		}
		[HttpPost]
		[Route("loginregister/logout")]
		[Route("/logout")]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}
}



