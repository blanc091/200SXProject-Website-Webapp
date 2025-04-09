using _200SXContact.Commands.Areas.MaintenApp;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.DTOs.Areas.MaintenApp;
using _200SXContact.Queries.Areas.MaintenApp;
using System.Globalization;

namespace _200SXContact.Controllers.Areas.MaintenApp
{
    public class DashboardController : Controller
	{
		private readonly ILoggerService _loggerService;
		private readonly IMediator _mediator;
		public DashboardController(ILoggerService loggerService, IMediator mediator)
		{
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

                TempData["Message"] = "Could not parse datetime format !";
                TempData["IsEntrySuccess"] = "no";

                return RedirectToAction("Dashboard", "Dashboard");
            }

            ReminderItemDto remindersDto = new ReminderItemDto
            {
                EntryItem = entryTitle,
                EntryDescription = entryDescription,
                DueDate = parsedDueDate
            };

            CreateEntryCommandHandler.CreateEntryCommandResult result = await _mediator.Send(new CreateEntryCommand(User, remindersDto));

            if (!result.Succeeded)
            {
                TempData["Message"] = string.Join("; ", result.Errors.SelectMany(e => e.Value));
                TempData["IsEntrySuccess"] = "no";

                return RedirectToAction("Dashboard", "Dashboard");
            }

            TempData["Message"] = "Entry created successfully !";
            TempData["IsEntrySuccess"] = "yes";

            return RedirectToAction("Dashboard", "Dashboard");
        }
		[HttpPost]
		[Route("update-entry")]
        [Consumes("application/json")]
        public async Task<JsonResult> UpdateEntry([FromBody] ReminderItemDto updatedItem)
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

                UpdateEntryCommandHandler.UpdateEntryCommandResult result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
                else
                {
                    string errorMsg = string.Join("; ", result.Errors.SelectMany(e => e.Value));
                    TempData["Message"] = errorMsg;
                    TempData["IsEntrySuccess"] = "no";

                    return Json(new { success = false, message = errorMsg });
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"MaintenApp || Exception: {ex.Message}", "Error", "");
                TempData["IsEntrySuccess"] = "no";

                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
