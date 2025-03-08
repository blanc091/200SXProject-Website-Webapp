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
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Extensions.Options;
using _200SXContact.Models.Areas.Newsletter;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Controllers.Account
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
			EmailSettings emailSettings = appSettings.Value.EmailSettings;
			_credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
			_configuration = configuration;
		}
		[HttpGet]
		[Route("login-with-microsoft")]
		[AllowAnonymous]
		public IActionResult LoginMicrosoft()
		{
            _loggerService.LogAsync("Login Register || Started Microsoft logging in process", "Info", "");
			string? redirectUri = Url.Action("SigninMicrosoft", "LoginRegister", null, Request.Scheme);
			AuthenticationProperties properties = new AuthenticationProperties { RedirectUri = redirectUri };
			_loggerService.LogAsync($"Login Register || Initiating Microsoft login, redirecting to: {redirectUri}", "Info", "");

			return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
		}
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> SigninMicrosoft()
		{
            await _loggerService.LogAsync("Login Register || Started Microsoft OAuth process", "Info", "");
			AuthenticateResult result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

			if (!result.Succeeded)
			{
				await _loggerService.LogAsync("Login Register || Microsoft login failed", "Error","");
				return RedirectToAction("Login");
			}

			Claim? emailClaim = result.Principal.FindFirst(ClaimTypes.Email);
			string? email = emailClaim?.Value;
			string username = email.Split('@')[0];
			username = Regex.Replace(username, @"[^a-zA-Z0-9]", string.Empty);
			string? timeZoneCookie = Request.Cookies["userTimeZone"];
			TimeZoneInfo? userTimeZone = null;

			if (!string.IsNullOrEmpty(timeZoneCookie))
			{
				try
				{
					userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneCookie);
				}
				catch (TimeZoneNotFoundException)
				{
					await _loggerService.LogAsync($"Invalid time zone received: {timeZoneCookie}", "Error", "");
				}
			}

			DateTime nowUtc = DateTime.UtcNow;
			DateTime nowLocal = userTimeZone != null ? TimeZoneInfo.ConvertTimeFromUtc(nowUtc, userTimeZone) : nowUtc;

			if (string.IsNullOrEmpty(email))
			{
				await _loggerService.LogAsync("Login Register || Email claim not found in the authentication result", "Error", "");
				return RedirectToAction("Login");
			}

			User? user = await _userManager.FindByEmailAsync(email);

			if (user == null)
			{
				user = new User
				{
					UserName = username,
					Email = email,
					EmailConfirmed = true,
					CreatedAt = nowLocal,
					IsEmailVerified = true
				};
				IdentityResult createUserResult = await _userManager.CreateAsync(user);

				if (!createUserResult.Succeeded)
				{
					await _loggerService.LogAsync("Login Register || Failed to create user: {Errors}" + string.Join(", ", createUserResult.Errors.Select(e => e.Description)), "Error","");
					return RedirectToAction("Login");
				}
			}

			user.LastLogin = nowLocal;
			IdentityResult updateResult = await _userManager.UpdateAsync(user);
			await _signInManager.SignInAsync(user, isPersistent: true);
			TempData["IsUserLoggedIn"] = true;
			ViewData["IsUserLoggedIn"] = true;
			TempData["MicrosoftLogin"] = true;
			ViewData["MessageLoginMicrosoft"] = "Logged in successfully with Microsoft !";
            await _loggerService.LogAsync("Login Register || Finished Microsoft logging in process", "Info", "");

            return RedirectToAction("Dashboard", "Dashboard");
		}
		[HttpGet]
		[Route("login-page")]
		[AllowAnonymous]
		public IActionResult Login(string returnUrl = null)
		{
            _loggerService.LogAsync("Login Register || Getting login page", "Info", "");

            if (returnUrl != null && !Url.IsLocalUrl(returnUrl))
			{
                _loggerService.LogAsync("Login Register || No return url when getting login page", "Error", "");
                returnUrl = null;
			}

			if (!User.Identity.IsAuthenticated)
			{
				ViewData["ReturnUrl"] = returnUrl;
                _loggerService.LogAsync("Login Register || Got login page", "Info", "");

                return View("~/Views/Account/Login.cshtml");
			}
			else
			{
                _loggerService.LogAsync("Login Register || User is already logged in, redirecting to MaintenApp", "Info", "");

                return RedirectToAction("Dashboard", "Dashboard");
			}
		}
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            await _loggerService.LogAsync("Login Register || Starting login process", "Info", "");
            returnUrl = returnUrl ?? Url.Action("Dashboard", "Dashboard");

            if (!ModelState.IsValid)
            {
                await _loggerService.LogAsync("Login Register || Model state invalid in login process", "Error", "");
                return View("~/Views/Account/Login.cshtml", model);
            }

			User? user = await _userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                if (!user.IsEmailVerified)
                {
                    ModelState.AddModelError("", "Please verify your email before logging in.");
                    TempData["Message"] = "User is not verified ! Check email.";
                    TempData["IsUserVerified"] = "no";
                    await _loggerService.LogAsync("Login Register || User email is not verified when trying to log in", "Error", "");
                    return View("~/Views/Account/Login.cshtml", model);
                }

				bool passwordVerificationResult = await _userManager.CheckPasswordAsync(user, model.Password);

                if (passwordVerificationResult)
                {
					List<Claim> claims = new List<Claim>
						{
							new Claim(ClaimTypes.Name, user.UserName),
							new Claim(ClaimTypes.Email, user.Email),
							new Claim(ClaimTypes.NameIdentifier, user.Id)
						};
					IList<string> roles = await _userManager.GetRolesAsync(user);

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

					ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
					Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
						string? userTimeZone = Request.Cookies["userTimeZone"];

						if (!string.IsNullOrEmpty(userTimeZone))
						{
							try
							{
								TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
								user.LastLogin = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
							}
							catch (TimeZoneNotFoundException)
							{
								user.LastLogin = DateTime.UtcNow;
							}
						}
						else
						{
							user.LastLogin = DateTime.UtcNow;
						}

						IdentityResult updateResult = await _userManager.UpdateAsync(user);
                        await _loggerService.LogAsync("Login Register || User logged in successfully", "Info", "");

                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    TempData["Message"] = "Invalid username or password !";
                    TempData["IsUserVerified"] = "no";
                    await _loggerService.LogAsync("Login Register || Invalid password attempt", "Error", "");

                    return View("~/Views/Account/Login.cshtml", model);
                }
            }

            ModelState.AddModelError("", "Invalid username or password.");
            TempData["Message"] = "Invalid username or password !";
            TempData["IsUserVerified"] = "no";
            await _loggerService.LogAsync("Login Register || User not found", "Error", "");

            return View("~/Views/Account/Login.cshtml", model);
        }
        [HttpGet]
		[Route("forgot-my-password")]
		public IActionResult ForgotPassReset()
		{
            _loggerService.LogAsync("Login Register || Getting forgot pass view", "Info", "");
            var model = new ForgotPasswordViewModel();
			if (model == null)
			{
				model = new ForgotPasswordViewModel();
			}
            _loggerService.LogAsync("Login Register || Got forgot pass view", "Info", "");
            return View("~/Views/Account/ForgotPassReset.cshtml", model);
		}
		[HttpGet]
		[Route("register-new-account")]
		public IActionResult Register()
		{
            _loggerService.LogAsync("Login Register || Getting register account view", "Info", "");

            if (!User.Identity.IsAuthenticated)
			{
                _loggerService.LogAsync("Login Register || Got forgot pass view", "Info", "");

                return View("~/Views/Account/Register.cshtml", new RegisterViewModel());
			}
			else
			{
                _loggerService.LogAsync("Login Register || User is already authenticated when trying to register new user, redirecting to MaintenApp", "Info", "");
                
				return RedirectToAction("Dashboard", "Dashboard");
			}
		}
		[HttpPost]
		[Route("register-an-account")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model, string gRecaptchaResponseRegister)
		{
            await _loggerService.LogAsync("Login Register || Starting new user registration process", "Info", "");

            if (User.Identity.IsAuthenticated)
			{
                await _loggerService.LogAsync("Login Register || User already logged in when trying to register account in post action", "Info", "");

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

			User? existingUser = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Username);

			if (existingUser != null)
			{
                await _loggerService.LogAsync("User already exists when trying to register new user", "Error", "");
                TempData["UserExists"] = "yes";
				TempData["Message"] = "User already exists !";

				return View("~/Views/Account/Register.cshtml", model);
			}

			string? userTimeZone = Request.Cookies["userTimeZone"];
			DateTime createdAt = DateTime.UtcNow;

			if (!string.IsNullOrEmpty(userTimeZone))
			{
				try
				{
					TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
					createdAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
				}
				catch (TimeZoneNotFoundException)
				{
					createdAt = DateTime.UtcNow;
				}
			}

			User user = new User
			{
				UserName = model.Username,
				Email = model.Email,
				CreatedAt = createdAt,
				IsEmailVerified = false,
				EmailVerificationToken = Guid.NewGuid().ToString()
			};
			IdentityResult createResult = await _userManager.CreateAsync(user, model.Password);

			if (!createResult.Succeeded)
			{                
                foreach (var error in createResult.Errors)
				{
                    await _loggerService.LogAsync("Login Register || Error in user creation " + error.Description, "Error", "");
                    ModelState.AddModelError(string.Empty, error.Description);
				}

				return View("~/Views/Account/Register.cshtml", model);
            }

            if (model.SubscribeToNewsletter)
			{
                await _loggerService.LogAsync("Login Register || New user opted for newsletter registration", "Info", "");
                await Subscribe(model.Email);
			}

			string? verificationUrl = Url.Action("VerifyEmail", "LoginRegister", new { token = user.EmailVerificationToken }, Request.Scheme);
			await SendVerificationEmail(model.Email, verificationUrl);
			TempData["IsFormRegisterSuccess"] = "yes";
			TempData["Message"] = "Registration successful! Check your email for verification.";
            await _loggerService.LogAsync("Login Register || New user registered, redirecting to login page", "Info", "");

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
            await _loggerService.LogAsync("Login Register || Starting Subscribe method when registering new user", "Info", "");
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
				await _context.NewsletterSubscriptions.AddAsync(subscription);
			}
			else if (!existingSubscription.IsSubscribed)
			{
                await _loggerService.LogAsync("Login Register || User already subscribed to the newsletter when registering new user", "Error", "");
                existingSubscription.IsSubscribed = true;
				existingSubscription.SubscribedAt = DateTime.Now;
			}
			await _context.SaveChangesAsync();
            await _loggerService.LogAsync("Login Register || Finished Subscribe method when registering new user", "Info", "");
        }
		[HttpGet]
		[Route("reset-my-password")]
		public IActionResult ResetPassword(string token, string email)
		{
            _loggerService.LogAsync("Login Register || Getting reset password view", "Info", "");
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
			{
                _loggerService.LogAsync("Login Register || Invalid password reset token or email in reset password view", "Error", "");
                return BadRequest("Invalid password reset token or email.");
			}
			var model = new ResetPasswordViewModel
			{
				Token = token,
				Email = email
			};
            _loggerService.LogAsync("Login Register || Got reset password view", "Info", "");
            return View("~/Views/Account/ResetPass.cshtml", model);
		}
		[HttpPost]
		[Route("forgot-password")]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
            await _loggerService.LogAsync("Login Register || Starting the forgot password process", "Info", "");
            if (!ModelState.IsValid)
			{
                await _loggerService.LogAsync("Login Register || Model state invalid when submitting reset password", "Error", "");
                return View("~/Views/Account/ForgotPassReset.cshtml", model);
			}
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
                await _loggerService.LogAsync("Login Register || User is null when trying to reset password", "Error", "");
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
            await _loggerService.LogAsync("Login Register || Finished the reset password process", "Info", "");
            return Redirect("/login-page");
		}
		[HttpPost]
		[Route("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
            await _loggerService.LogAsync("Login Register || Started ResetPassword POST", "Info", "");
            if (!ModelState.IsValid)
			{
                await _loggerService.LogAsync("Login Register || Model state invalid in ResetPassword", "Error", "");
                return View("~/Views/Account/ResetPass.cshtml", model);
			}
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
                await _loggerService.LogAsync("Login Register || User is null or invalid email reset token in ResetPassword", "Error", "");
                ModelState.AddModelError("", "Invalid token or email.");
				return View("~/Views/Account/ResetPass.cshtml", model);
			}
			var decodedToken = System.Web.HttpUtility.UrlDecode(model.Token);
			var resetResult = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
			if (!resetResult.Succeeded)
			{
				foreach (var error in resetResult.Errors)
				{
                    await _loggerService.LogAsync("Login Register || Model error in ResetPassword " + error.Description, "Error", "");
                    ModelState.AddModelError(string.Empty, error.Description);
				}
				return View("~/Views/Account/ResetPass.cshtml", model);
			}
			TempData["PassResetSuccess"] = "yes";
			TempData["Message"] = "Your password has been reset successfully.";
            await _loggerService.LogAsync("Login Register || Finished ResetPassword POST", "Info", "");
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
            await _loggerService.LogAsync("Login Register || Starting sending password reset email task", "Info", "");
            var fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
			var toAddress = new MailAddress(email);
			string subject = "Import Garage || Reset Your Password";
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
                    await _loggerService.LogAsync("Login Register || Finished sending password reset email task", "Info", "");
                }
			}
		}
		private async Task SendVerificationEmail(string email, string verificationUrl)
		{
            await _loggerService.LogAsync("Login Register || Starting sending user verification email", "Info", "");
            var fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
			var toAddress = new MailAddress(email);
			string subject = "Import Garage || Verify your email";
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
                    await _loggerService.LogAsync("Login Register || Finished sending user verification email", "Info", "");
                }
			}
		}
		[HttpGet]
		public async Task<IActionResult> VerifyEmail(string token)
		{
            await _loggerService.LogAsync("Login Register || Starting VerifyEmail method for user creation", "Info", "");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
			if (user == null)
			{
                await _loggerService.LogAsync("Login Register || User is null or invalid token in VerifyEmail method for user creation", "Error", "");
                return NotFound("Invalid token.");
			}
			user.IsEmailVerified = true;
			user.EmailVerificationToken = null;
			user.EmailConfirmed = true;
			await _userManager.UpdateAsync(user);
			TempData["IsEmailVerifiedLogin"] = "yes";
			TempData["Message"] = "Email verified ! You can now log in.";
            await _loggerService.LogAsync("Login Register || Finished VerifyEmail method for user creation", "Info", "");
            return Redirect("/login-page");
		}
		[HttpPost]
		[Route("loginregister/logout")]
		[Route("/logout")]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
            await _loggerService.LogAsync("Login Register || Starting logging out", "Info", "");
            await _signInManager.SignOutAsync();
            await _loggerService.LogAsync("Login Register || User logged out", "Info", "");
            return RedirectToAction("Index", "Home");
		}
	}
}



