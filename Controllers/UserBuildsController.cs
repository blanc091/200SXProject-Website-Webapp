using _200SXContact.Models;
using _200SXContact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace _200SXContact.Controllers
{
	public class UserBuildsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<UserBuildsController> _logger;
		private readonly UserManager<User> _userManager;
		public UserBuildsController(ApplicationDbContext context, ILogger<UserBuildsController> logger, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
			_logger = logger;
		}
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> AddUserBuild()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = new UserBuild
			{
				UserName = user?.UserName
			};
			return View("~/Views/UserContent/AddUserBuild.cshtml", model);
		}
		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SubmitBuild(UserBuild model, IFormFile image)
		{
			if (ModelState.IsValid)
			{
				if (image != null && image.Length > 0)
				{
					var user = await _userManager.GetUserAsync(User);
					var userEmail = user.Email.Replace("@", "_at_").Replace(".", "_");
					var userDirectory = Path.Combine("wwwroot/images/uploads", userEmail);
					if (!Directory.Exists(userDirectory))
					{
						Directory.CreateDirectory(userDirectory);
					}

					var imagePath = Path.Combine(userDirectory, image.FileName);
					using (var stream = new FileStream(imagePath, FileMode.Create))
					{
						await image.CopyToAsync(stream);
					}

					model.ImagePath = $"/images/uploads/{userEmail}/{image.FileName}";
					model.DateCreated = DateTime.Now;
					model.UserEmail = user.Email;
				}

				_context.UserBuilds.Add(model);
				await _context.SaveChangesAsync();

				return RedirectToAction("UserContentDashboard", "UserContent");
			}
			return View(model);
		}
		[HttpGet]
		public async Task<IActionResult> DetailedUserView(int id)
		{
			var build = await _context.UserBuilds
				.FirstOrDefaultAsync(b => b.Id == id);
			if (build == null)
			{
				return NotFound(); 
			}
			return View("~/Views/UserContent/DetailedUserView.cshtml", build); 
		}
		[HttpGet]
		public async Task<IActionResult> UserContentDashboard()
		{
			var builds = await _context.UserBuilds
					   .OrderByDescending(b => b.DateCreated)
					   .ToListAsync();
			return View("~/Views/UserContent/UserContentDashboard.cshtml", builds);
		}
	}
}
