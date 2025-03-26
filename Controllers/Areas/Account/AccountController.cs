using _200SXContact.Commands.Areas.Account;
using _200SXContact.Commands.Areas.Admin;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.Configs;
using _200SXContact.Models.DTOs.Areas.Account;
using _200SXContact.Models.DTOs.Areas.Chat;
using _200SXContact.Queries.Areas.Account;
using System.Net;

namespace _200SXContact.Controllers.Areas.Account
{
    public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly IApplicationDbContext _context;
		private readonly SignInManager<User> _signInManager;
		private readonly ILoggerService _loggerService;		
		private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public AccountController(IOptions<AppSettings> appSettings, IMapper mapper, IMediator mediator, IApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, ILoggerService loggerService)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
			_loggerService = loggerService;
            EmailSettings emailSettings = appSettings.Value.EmailSettings;
			_mediator = mediator;
            _mapper = mapper;
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
        public async Task<IActionResult> GetPendingChatSessions()
        {
            await _loggerService.LogAsync("Account || Getting chat sessions for admin dash page", "Info", "");

            List<Models.Areas.Chat.ChatSession> pendingSessions = await _context.ChatSessions.Where(cs => cs.IsAnswered == false).ToListAsync();

            List<ChatSessionDto> pendingSessionDtos = _mapper.Map<List<ChatSessionDto>>(pendingSessions);

            await _loggerService.LogAsync("Account || Got chat sessions for admin dash page", "Info", "");

            return Json(pendingSessionDtos);
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
                UserManager<User> userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                User? user = await userManager.GetUserAsync(User);

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
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            UserProfileDto viewModel = await _mediator.Send(new GetUserProfileQuery { UserId = userId });

            if (viewModel == null)
            {
				TempData["Message"] = "You need to log in first !";
				TempData["IsUserLoggedIn"] = "no";
                RedirectToAction("LoginRegister", "Login");
            }

            return View("~/Views/Account/UserDash.cshtml", viewModel);
        }
		[HttpGet]
		[Route("account/create-test-user")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateTestUser(string userName = "testAccount", string password = "Test@cc34")
		{
            bool success = await _mediator.Send(new CreateTestUserCommand
            {
                UserName = userName,
                Password = password
            });

            if (success)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Message"] = "Test user creation failed or user already exists.";

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
            User? user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                TempData["Message"] = "User does not exist !";

                return RedirectToAction("DeleteAccountConfirmation", "Account");
            }

            string token = await _userManager.GenerateUserTokenAsync(user, "Default", "DeleteAccountToken");
            string encodedToken = WebUtility.UrlEncode(token);
            string? resetUrl = Url.Action("DeleteAccount", "Account", new { email = userEmail, token = encodedToken }, Request.Scheme);

            bool success = await _mediator.Send(new DeleteAccountVerifyCommand { UserEmail = userEmail, ResetUrl = resetUrl });

            if (success)
            {
                TempData["Message"] = "Account deletion email sent.";
                TempData["UserDeleteAccEmailSent"] = "yes";
            }
            else
            {
                TempData["Message"] = "User does not exist !";
            }

            return RedirectToAction("DeleteAccountConfirmation", "Account");
        }
		//[HttpPost]
		[Route("account/delete-account")]
		//[Authorize]
		public async Task<IActionResult> DeleteAccount(string email, string token)
		{
            await _loggerService.LogAsync("Account || Deleting user in profile page (Controller)", "Info", "");

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found.");
            }

            bool success = await _mediator.Send(new DeleteAccountCommand
            {
                Email = email,
                Token = token,
                UserId = userId
            });

            if (success)
            {
                TempData["Message"] = "Account deletion completed successfully.";
                TempData["UserDeletedAcc"] = "yes";

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting your account.");
                TempData["Message"] = "Account deletion failed, please contact us !";
                TempData["UserDeletedAcc"] = "no";

                return RedirectToAction("UserProfile", "Account");
            }
        }
	}
}
