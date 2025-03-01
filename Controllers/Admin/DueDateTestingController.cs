using _200SXContact.Interfaces;
using _200SXContact.Services;
using Microsoft.AspNetCore.Mvc;

namespace _200SXContact.Controllers.Admin
{
	[Route("DueDateTesting")]
	public class DueDateTestingController : Controller
	{
        private readonly IDueDateReminderService _dueDateReminderService;
        private readonly ILoggerService _loggerService;
		public DueDateTestingController(IDueDateReminderService dueDateReminderService, ILoggerService loggerService)
		{
			_dueDateReminderService = dueDateReminderService;
			_loggerService = loggerService;
		}
		[HttpGet("ManualCheckDueDates")]
		public async Task<IActionResult> ManualCheckDueDates()
		{
            await _loggerService.LogAsync("Due Date Reminder || Starting manually checking due dates", "Info", "");
            await _dueDateReminderService.ManualCheckDueDates();
            await _loggerService.LogAsync("Due Date Reminder || Finished manually checking due dates", "Info", "");
            return Ok("Manual check of due dates initiated.");
		}
	}
}
