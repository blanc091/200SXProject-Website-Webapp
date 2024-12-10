using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace _200SXContact.Controllers
{
	public class DashboardController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly ILoggerService _loggerService;
		public DashboardController(ApplicationDbContext context, UserManager<User> userManager, ILoggerService loggerService)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
		}
		[HttpGet]
		[Route("mainten-app")]		
		public async Task<IActionResult> Dashboard()
		{
            await _loggerService.LogAsync("MaintenApp || Started getting MaintenApp dash view", "Info", "");
            if (!User.Identity.IsAuthenticated)
			{
                await _loggerService.LogAsync("MaintenApp || User not authenticated when getting MaintenApp dash view", "Error", "");
                return Redirect("/login-page");
			}
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
                await _loggerService.LogAsync("MaintenApp || User is null or cannot retrieve user when getting MaintenApp dash view", "Error", "");
                return NotFound("User not found");
			}
			var userWithItems = await _context.Users
											  .Include(u => u.Items)
											  .FirstOrDefaultAsync(u => u.Id == user.Id);
			if (userWithItems == null || !userWithItems.Items.Any())
			{
                await _loggerService.LogAsync("MaintenApp || No items forund for the user in MaintenApp dash view", "Info", "");
                return View("~/Views/Account/Dashboard.cshtml", new List<Item>()); 
			}
			var items = userWithItems.Items.ToList();
            await _loggerService.LogAsync("MaintenApp || Got MaintenApp dash view", "Info", "");
            return View("~/Views/Account/Dashboard.cshtml", items);
		}
		[HttpPost]
		[Route("add-entry")]
		public async Task<IActionResult> CreateEntry(string entryTitle, string entryDescription, string dueDate)
		{
            await _loggerService.LogAsync("MaintenApp || Started adding entry in MaintenApp dash view", "Info", "");;
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
			if (user == null)
			{
                await _loggerService.LogAsync("MaintenApp || User is null when creating entry in MaintenApp dash view", "Error", "");
                return NotFound("User not found");
			}
			string[] dateFormats = { "MM/dd/yyyy", "yyyy-MM-dd" };
			if (!DateTime.TryParseExact(dueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDueDate))
			{
                await _loggerService.LogAsync("MaintenApp || Could not parse datetime format when creating user entry in MaintenApp dash view", "Error", "");
                return BadRequest("Invalid due date format");
			}
			TempData["IsUserLoggedIn"] = true;
			var newItem = new Item
			{
				EntryItem = entryTitle,
				EntryDescription = entryDescription,
				DueDate = parsedDueDate,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				UserId = user.Id
			};
			await _context.Items.AddAsync(newItem);
			await _context.SaveChangesAsync();
			TempData["Message"] = "Entry created successfully !";
			TempData["IsEntrySuccess"] = "yes";
            await _loggerService.LogAsync("MaintenApp || Finished adding entry in MaintenApp dash view", "Info", "");
            return RedirectToAction("Dashboard", "Dashboard");
		}
		[HttpPost]
		[Route("update-entry")]
		public JsonResult UpdateEntry([FromBody] Item updatedItem)
		{
			try
			{
                _loggerService.LogAsync("MaintenApp || Started updating entry in MaintenApp dash view", "Info", "");
                var existingItem = _context.Items.FirstOrDefault(i => i.Id == updatedItem.Id);
				if (existingItem != null)
				{
					existingItem.EntryItem = updatedItem.EntryItem;
					existingItem.EntryDescription = updatedItem.EntryDescription;
					existingItem.DueDate = updatedItem.DueDate;
					existingItem.UpdatedAt = DateTime.Now;
					_context.SaveChangesAsync();
                    _loggerService.LogAsync("MaintenApp || Finished updating entry in MaintenApp dash view", "Info", "");
                    return Json(new { success = true });
				}
				else
				{
                    _loggerService.LogAsync("MaintenApp || Item not found when updating entry in MaintenApp dash view", "Error", "");
                    return Json(new { success = false, message = "Item not found" });
				}
			}
			catch (Exception ex)
			{
                _loggerService.LogAsync("MaintenApp || Unknown exception when updating entry in MaintenApp dash view " + ex.Message, "Error", "");
                return Json(new { success = false, message = ex.Message });
			}
		}		
	}
}
