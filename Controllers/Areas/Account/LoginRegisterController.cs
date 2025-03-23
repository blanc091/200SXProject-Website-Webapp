using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using _200SXContact.Models.Configs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.Areas.Account;
using _200SXContact.Commands.Areas.Account;
using _200SXContact.Models.DTOs.Areas.Account;
using AutoMapper;
using MediatR;

namespace _200SXContact.Controllers.Areas.Account
{
    public class LoginRegisterController : Controller
	{
		private readonly SignInManager<User> _signInManager;
		private readonly ILoggerService _loggerService;
		private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public LoginRegisterController(IOptions<AppSettings> appSettings, ILoggerService loggerService, SignInManager<User> signInManager, UserManager<User> userManager, IMediator mediator, IMapper mapper)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_loggerService = loggerService;
            _mapper = mapper;
            _mediator = mediator;
		}
		[HttpGet]
		[Route("login-with-microsoft")]
		[AllowAnonymous]
		public async Task<IActionResult> LoginMicrosoft()
		{
            await _loggerService.LogAsync("Login Register || Started Microsoft logging in process", "Info", "");

			string? redirectUri = Url.Action("SigninMicrosoft", "LoginRegister", null, Request.Scheme);
			AuthenticationProperties properties = new AuthenticationProperties { RedirectUri = redirectUri };

            await _loggerService.LogAsync($"Login Register || Initiating Microsoft login, redirecting to: {redirectUri}", "Info", "");

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
		public async Task<IActionResult> Login(string returnUrl = null)
		{
            await _loggerService.LogAsync("Login Register || Getting login page", "Info", "");

            if (returnUrl != null && !Url.IsLocalUrl(returnUrl))
			{
                await _loggerService.LogAsync("Login Register || No return url when getting login page", "Error", "");

                returnUrl = null;
			}

			if (!User.Identity.IsAuthenticated)
			{
				ViewData["ReturnUrl"] = returnUrl;

                await _loggerService.LogAsync("Login Register || Got login page", "Info", "");

                return View("~/Views/Account/Login.cshtml");
			}
			else
			{
                await _loggerService.LogAsync("Login Register || User is already logged in, redirecting to MaintenApp", "Info", "");

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

                    foreach (string role in roles)
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
		public async Task<IActionResult> ForgotPassReset()
		{
            await _loggerService.LogAsync("Login Register || Getting forgot pass view", "Info", "");

            ForgotPasswordViewModel model = new ForgotPasswordViewModel();

			if (model == null)
			{
				model = new ForgotPasswordViewModel();
			}

            await _loggerService.LogAsync("Login Register || Got forgot pass view", "Info", "");

            return View("~/Views/Account/ForgotPassReset.cshtml", model);
		}
		[HttpGet]
		[Route("register-new-account")]
		public async Task<IActionResult> Register()
		{
            await _loggerService.LogAsync("Login Register || Getting register account view", "Info", "");

            if (!User.Identity.IsAuthenticated)
			{
                await _loggerService.LogAsync("Login Register || Got forgot pass view", "Info", "");

                return View("~/Views/Account/Register.cshtml", new RegisterViewModel
                {
                    Username = string.Empty,
                    Password = string.Empty,
                    Email = string.Empty
                });
            }
			else
			{
                await _loggerService.LogAsync("Login Register || User is already authenticated when trying to register new user, redirecting to MaintenApp", "Info", "");
                
				return RedirectToAction("Dashboard", "Dashboard");
			}
		}
		[HttpPost]
		[Route("register-an-account")]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string gRecaptchaResponseRegister)
        {
            if (!ModelState.IsValid)
            {
                TempData["IsFormRegisterSuccess"] = "no";

                return View("~/Views/Account/Register.cshtml", model);
            }

            ExtendedRegisterDto extendedDto = new ExtendedRegisterDto
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                SubscribeToNewsletter = model.SubscribeToNewsletter,
                honeypotSpam = model.honeypotSpam,
                RecaptchaResponse = gRecaptchaResponseRegister,
                TimeZoneCookie = Request.Cookies["userTimeZone"],
                IsCalledFromRegisterForm = true
            };

            RegisterUserCommand command = _mapper.Map<RegisterUserCommand>(extendedDto);
            string userToken = Guid.NewGuid().ToString();
            string encodedToken = System.Web.HttpUtility.UrlEncode(userToken);
            string verificationUrl = Url.Action("VerifyEmail", "LoginRegister", new { token = encodedToken, email = extendedDto.Email }, Request.Scheme);
            command.VerificationUrl = verificationUrl;
            command.VerificationToken = encodedToken;
            RegisterUserCommandResult result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                foreach (string error in result.Errors)
                {
                    TempData["IsFormRegisterSuccess"] = "no";
                    ModelState.AddModelError(string.Empty, error);
                }

                return View("~/Views/Account/Register.cshtml", model);
            }

            TempData["IsFormRegisterSuccess"] = "yes";
            TempData["Message"] = "Registration successful ! Check your email for verification.";

            return Redirect("/login-page");
        }
		[HttpGet]
		[Route("reset-my-password")]
		public async Task<IActionResult> ResetPassword(string token, string email)
		{
            await _loggerService.LogAsync("Login Register || Getting reset password view", "Info", "");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
			{
                await _loggerService.LogAsync("Login Register || Invalid password reset token or email in reset password view", "Error", "");

                return BadRequest("Invalid password reset token or email.");
			}

            ResetPasswordViewModel model = new ResetPasswordViewModel
			{
				Token = token,
				Email = email
			};

            await _loggerService.LogAsync("Login Register || Got reset password view", "Info", "");

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

            User? user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                await _loggerService.LogAsync("Login Register || User is null when trying to reset password", "Error", "");

                TempData["Message"] = "If that email is associated with an account, you will receive a password reset link.";

                return RedirectToAction("ForgotPassword");
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string encodedToken = System.Web.HttpUtility.UrlEncode(token);
            string? resetUrl = Url.Action("ResetPassword", "LoginRegister", new { token = encodedToken, email = user.Email }, Request.Scheme);
            ForgotPasswordCommand command = new ForgotPasswordCommand
            {
                Email = user.Email,
                ResetUrl = resetUrl
            };

            ForgotPasswordCommandResult result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                TempData["Message"] = "There was an error processing your request.";

                await _loggerService.LogAsync("Login Register || Error processing the request when trying to reset password", "Error", "");

                return RedirectToAction("ForgotPassword");
            }

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

            ResetPasswordCommand command = new ResetPasswordCommand
            {
                Email = model.Email,
                Token = model.Token,
                NewPassword = model.NewPassword
            };

            ResetPasswordCommandResult result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                foreach (string error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
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
		[HttpGet]
		public async Task<IActionResult> VerifyEmail(string token)
		{
            await _loggerService.LogAsync("Login Register || Starting VerifyEmail method for user creation", "Info", "");

            VerifyEmailCommand command = new VerifyEmailCommand { Token = token };
            VerifyEmailCommandResult result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                await _loggerService.LogAsync("Login Register || Verification failed: " + result.Error, "Error", "");

                return NotFound(result.Error);
            }

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



