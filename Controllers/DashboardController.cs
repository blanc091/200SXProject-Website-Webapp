using _200SXContact.Data;
using _200SXContact.Models;
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
		public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<IActionResult> Dashboard()
		{
			if (!User.Identity.IsAuthenticated)
			{
				Console.WriteLine("User is not authenticated");
				return RedirectToAction("Login", "LoginRegister");
			}
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				Console.WriteLine("User is null, cannot retrieve user");
				return NotFound("User not found");
			}
			var userWithItems = await _context.Users
											  .Include(u => u.Items)
											  .FirstOrDefaultAsync(u => u.Id == user.Id);

			if (userWithItems == null || !userWithItems.Items.Any())
			{
				Console.WriteLine("No items found for this user");
				return View("~/Views/Account/Dashboard.cshtml", new List<Item>()); 
			}

			var items = userWithItems.Items.ToList();
			return View("~/Views/Account/Dashboard.cshtml", items);
		}
		[HttpPost]
		public async Task<IActionResult> CreateEntry(string entryTitle, string entryDescription, string dueDate)
		{
			Console.WriteLine("Received Due Date: " + dueDate);

			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

			if (user == null)
			{
				return NotFound("User not found");
			}

			string[] dateFormats = { "MM/dd/yyyy", "yyyy-MM-dd" };
			if (!DateTime.TryParseExact(dueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDueDate))
			{
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

			_context.Items.Add(newItem);
			await _context.SaveChangesAsync();

			TempData["Message"] = "Entry created successfully !";
			TempData["IsEntrySuccess"] = "yes";
			return RedirectToAction("Dashboard", "Dashboard");
		}
		[HttpPost]
		public JsonResult UpdateEntry([FromBody] Item updatedItem)
		{
			try
			{
				var existingItem = _context.Items.FirstOrDefault(i => i.Id == updatedItem.Id);
				if (existingItem != null)
				{
					existingItem.EntryItem = updatedItem.EntryItem;
					existingItem.EntryDescription = updatedItem.EntryDescription;
					existingItem.DueDate = updatedItem.DueDate;
					existingItem.UpdatedAt = DateTime.Now;
					_context.SaveChanges();

					return Json(new { success = true });
				}
				else
				{
					return Json(new { success = false, message = "Item not found" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
		[HttpGet]
		public IActionResult GetEntries()
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var entries = _context.Items
				.Where(item => item.UserId == userId)
				.OrderByDescending(item => item.CreatedAt)
				.ToList();
			return View("~/Views/Account/Dashboard.cshtml", entries);
		}
		private int GetLoggedInUserId()
		{
			return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
		}
	}
}
