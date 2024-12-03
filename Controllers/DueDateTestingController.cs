using _200SXContact.Services;
using Microsoft.AspNetCore.Mvc;

namespace _200SXContact.Controllers
{
	[Route("DueDateTesting")]
	public class DueDateTestingController : Controller
	{
		private readonly DueDateReminderService _dueDateReminderService;

		public DueDateTestingController(DueDateReminderService dueDateReminderService)
		{
			_dueDateReminderService = dueDateReminderService;
		}
		[HttpGet("ManualCheckDueDates")]
		public async Task<IActionResult> ManualCheckDueDates()
		{
			await _dueDateReminderService.ManualCheckDueDates();
			return Ok("Manual check of due dates initiated.");
		}
	}
}
