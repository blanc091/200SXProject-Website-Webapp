using _200SXContact.Models;
using _200SXContact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using _200SXContact.Interfaces.Areas.Admin;

namespace _200SXContact.Controllers.Users
{
	public class UserBuildsController : Controller
	{
		private readonly ApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly UserManager<User> _userManager;
		public UserBuildsController(ApplicationDbContext context, ILoggerService loggerService, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
		}
		[HttpGet]
		[Route("add-new-build")]
		[Authorize]
		public async Task<IActionResult> AddUserBuild()
		{
			await _loggerService.LogAsync("User builds || Getting Add User Build Interface", "Info", "");
			var user = await _userManager.GetUserAsync(User);
			var model = new UserBuild
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
		public async Task<IActionResult> SubmitBuild(UserBuild model, IFormFile[] Images)
		{
            await _loggerService.LogAsync("User builds || Starting to submit user build", "Info", "");
            ModelState.Remove("ImagePath");
			//ModelState.Remove("Id");
			model.Id = Guid.NewGuid().ToString();
			var user = await _userManager.GetUserAsync(User);
			if (ModelState.IsValid)
			{
				List<string> imagePaths = new List<string>();
				if (Images != null && Images.Length > 0)
				{
					foreach (var image in Images)
					{
						if (image.Length > 0)
						{
							var userEmail = user.Email.Replace("@", "_at_").Replace(".", "_");
							var userDirectory = Path.Combine("wwwroot/images/uploads", user.Id);
							if (!Directory.Exists(userDirectory))
							{
								Directory.CreateDirectory(userDirectory);
							}
							var imagePath = Path.Combine(userDirectory, image.FileName);
							using (var stream = new FileStream(imagePath, FileMode.Create))
							{
								await image.CopyToAsync(stream);
							}
							imagePaths.Add($"/images/uploads/{user.Id}/{image.FileName}");
						}
					}
					model.ImagePaths = imagePaths; 
					model.DateCreated = DateTime.Now;
					model.UserEmail = user.Email;
					model.UserName = user.UserName;
					model.UserId = user.Id;
				}
				await _context.UserBuilds.AddAsync(model);
				await _context.SaveChangesAsync();
                await _loggerService.LogAsync("User builds || Submitted User Build for " + model.UserId, "Info", "");
                return RedirectToAction("UserContentDashboard", "UserBuilds");
			}
			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
                await _loggerService.LogAsync("User builds || Error in model state at AddUserBuild: " + error.ErrorMessage, "Error", "");
			}
			return View("~/Views/UserContent/AddUserBuild.cshtml", model);
		}
		[HttpGet]
		[Route("detailed-user-build")]
		public async Task<IActionResult> DetailedUserView(string id)
		{
            await _loggerService.LogAsync("User builds || Getting detailed user build interface", "Info", "");
            var build = await _context.UserBuilds
					   .Include(b => b.Comments) 
					   .FirstOrDefaultAsync(b => b.Id == id);
			if (build == null)
			{
                await _loggerService.LogAsync("User builds || Error in DetailedUserView, build not found", "Error", "");
                return NotFound(); 
			}
            await _loggerService.LogAsync("User builds || Got detailed user build interface", "Info", "");
            return View("~/Views/UserContent/DetailedUserView.cshtml", build); 
		}
		[HttpGet]
		[Route("user-builds")]
		public async Task<IActionResult> UserContentDashboard()
		{
            await _loggerService.LogAsync("User builds || Getting user builds dashboard", "Info", "");
            var builds = await _context.UserBuilds
					   .OrderByDescending(b => b.DateCreated)
					   .ToListAsync();
            await _loggerService.LogAsync("User builds || Got user builds dashboard", "Info", "");
            return View("~/Views/UserContent/UserContentDashboard.cshtml", builds);
		}		
	}
}
