using _200SXContact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.UserContent;
using _200SXContact.Commands.Areas.UserContent;
using System.Security.Claims;
using MediatR;
using _200SXContact.Queries.Areas.UserContent;

namespace _200SXContact.Controllers.Users
{
	public class UserBuildsController : Controller
	{
		private readonly ApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly UserManager<User> _userManager;
		private readonly IMediator _mediator;
		public UserBuildsController(ApplicationDbContext context, ILoggerService loggerService, UserManager<User> userManager, IMediator mediator)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
			_mediator = mediator;
		}
		[HttpGet]
		[Route("add-new-build")]
		[Authorize]
		public async Task<IActionResult> AddUserBuild()
		{
			await _loggerService.LogAsync("User builds || Getting Add User Build Interface", "Info", "");

            User? user = await _userManager.GetUserAsync(User);
            UserBuildDto model = new UserBuildDto
			{
				UserName = user.UserName
			};

            await _loggerService.LogAsync("User builds || Got Add User Build Interface", "Info", "");

            return View("~/Views/UserContent/AddUserBuild.cshtml", model);
		}
		[HttpPost]
		[Route("submit-build")]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SubmitBuild(UserBuildDto model, IFormFile[] Images)
		{
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                await _loggerService.LogAsync("User builds || No userId found when trying to submit user build.", "Error", "");

                return Forbid();
            }

            SubmitBuildResult result = await _mediator.Send(new SubmitBuildCommand { UserId = userId, Model = model, Images = Images });

            switch (result)
            {
                case SubmitBuildResult.UserNotFound:

                    await _loggerService.LogAsync("User builds || User not found", "Error", "");

                    return Forbid();
                case SubmitBuildResult.InvalidImage:

                    await _loggerService.LogAsync("User builds || Invalid image", "Error", "");

                    ModelState.AddModelError("", "One or more images are invalid.");
                    break;
                case SubmitBuildResult.NoTitle:

                    await _loggerService.LogAsync("User builds || Invalid title", "Error", "");

                    ModelState.AddModelError("", "Please enter a title for the build.");
                    break;
                case SubmitBuildResult.NoDescription:

                    await _loggerService.LogAsync("User builds || Invalid description", "Error", "");

                    ModelState.AddModelError("", "Please enter a description for the build.");
                    break;
                case SubmitBuildResult.Success:
                    return RedirectToAction("UserContentDashboard", "UserBuilds");
            }

            return View("~/Views/UserContent/AddUserBuild.cshtml", model);
        }
		[HttpGet]
		[Route("detailed-user-build")]
		public async Task<IActionResult> DetailedUserView(string id)
		{
            UserBuildDto? build = await _mediator.Send(new GetUserBuildQuery { Id = id });

            if (build == null)
            {
                await _loggerService.LogAsync("User builds || Could not get detailed user build for user id " + id, "Error", "");

                return View("~/Views/UserContent/DetailedUserView.cshtml");
            }

            return View("~/Views/UserContent/DetailedUserView.cshtml", build);
        }
		[HttpGet]
		[Route("user-builds")]
		public async Task<IActionResult> UserContentDashboard()
		{
            List<UserBuildDto> builds = await _mediator.Send(new GetUserBuildsQuery());

            return View("~/Views/UserContent/UserContentDashboard.cshtml", builds);
        }		
	}
}
