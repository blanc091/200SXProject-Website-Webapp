using _200SXContact.Models;
using _200SXContact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;

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
				UserName = user.UserName
			};
			return View("~/Views/UserContent/AddUserBuild.cshtml", model);
		}
		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SubmitBuild(UserBuild model, IFormFile[] Images)
		{
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

				_context.UserBuilds.Add(model);
				await _context.SaveChangesAsync();

				return RedirectToAction("UserContentDashboard", "UserBuilds");
			}

			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
				_logger.LogError("Model Error: {0}", error.ErrorMessage);
			}
			return View("~/Views/UserContent/AddUserBuild.cshtml", model);
		}

		[HttpGet]
		public async Task<IActionResult> DetailedUserView(string id)
		{
			var build = await _context.UserBuilds
					   .Include(b => b.Comments) 
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
