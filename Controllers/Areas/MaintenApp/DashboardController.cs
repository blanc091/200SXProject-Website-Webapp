using _200SXContact.Commands.Areas.MaintenApp;
using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.MaintenApp;
using _200SXContact.Queries.Areas.MaintenApp;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace _200SXContact.Controllers.Areas.MaintenApp
{
    public class DashboardController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly ILoggerService _loggerService;
		private readonly IMediator _mediator;
		public DashboardController(ApplicationDbContext context, UserManager<User> userManager, ILoggerService loggerService, IMediator mediator)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
			_mediator = mediator;
		}
		[HttpGet]
		[Route("mainten-app")]		
		public async Task<IActionResult> Dashboard()
		{
            (GetUserDashboardResult result, List<ReminderItemDto> items) = await _mediator.Send(new GetUserDashboardQuery(User));

            switch (result)
            {
                case GetUserDashboardResult.UserNotAuthenticated:
					TempData["Message"] = "You need an account in order to use MaintenApp.";
					TempData["IsUserLoggedIn"] = "no";

                    return Redirect("/login-page");
                case GetUserDashboardResult.UserNotFound:

                    return NotFound("User not found");
                case GetUserDashboardResult.NoItemsFound:

                    return View("~/Views/MaintenApp/Dashboard.cshtml", new List<ReminderItemDto>());
                case GetUserDashboardResult.Success:
                default:

                    return View("~/Views/MaintenApp/Dashboard.cshtml", items);
            }
        }
		[HttpPost]
		[Route("add-entry")]
		public async Task<IActionResult> CreateEntry(string entryTitle, string entryDescription, string dueDate)
		{
            string[] dateFormats = { "MM/dd/yyyy", "yyyy-MM-dd" };

            if (!DateTime.TryParseExact(dueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDueDate))
            {
                await _loggerService.LogAsync("MaintenApp || Could not parse datetime format when creating user entry in MaintenApp dash view", "Error", "");

                return BadRequest("Invalid due date format");
            }

            ReminderItemDto remindersDto = new ReminderItemDto
            {
                EntryItem = entryTitle,
                EntryDescription = entryDescription,
                DueDate = parsedDueDate
            };

            CreateEntryResult result = await _mediator.Send(new CreateEntryCommand(User, remindersDto));

            switch (result)
            {
                case CreateEntryResult.UserNotFound:

                    return NotFound("User not found");
                case CreateEntryResult.InvalidDueDate:

                    return BadRequest("Invalid due date format");
                case CreateEntryResult.Success:
                    TempData["Message"] = "Entry created successfully!";
                    TempData["IsEntrySuccess"] = "yes";

                    return RedirectToAction("Dashboard", "Dashboard");
                default:
                    return StatusCode(500, "An unexpected error occurred.");
            }
        }
		[HttpPost]
		[Route("update-entry")]
        public async Task<JsonResult> UpdateEntry([FromBody] ReminderItem updatedItem)
        {
            try
            {
                await _loggerService.LogAsync("MaintenApp || Started updating entry in MaintenApp dash view", "Info", "");

                UpdateEntryCommand command = new UpdateEntryCommand
                {
                    Id = updatedItem.Id,
                    EntryItem = updatedItem.EntryItem,
                    EntryDescription = updatedItem.EntryDescription,
                    DueDate = updatedItem.DueDate
                };

                UpdateEntryResult result = await _mediator.Send(command);

                if (result == UpdateEntryResult.Success)
                {
                    return Json(new { success = true });
                }
                else
                {
                    TempData["Message"] = "Failed to update entry, please contact us !";
                    TempData["IsEntrySuccess"] = "no";

                    return Json(new { success = false, message = "Item not found or update failed" });
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"MaintenApp || Exception: {ex.Message}", "Error", "");

                return Json(new { success = false, message = ex.Message });
            }
        }		
	}
}
