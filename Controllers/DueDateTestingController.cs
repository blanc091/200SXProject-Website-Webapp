using _200SXContact.Services;
using Microsoft.AspNetCore.Mvc;

namespace _200SXContact.Controllers
{
	[Route("DueDateTesting")]  // This will make this controller accessible at /DueDateTesting
	public class DueDateTestingController : Controller
	{
		private readonly DueDateReminderService _dueDateReminderService;

		// Inject DueDateReminderService into the controller
		public DueDateTestingController(DueDateReminderService dueDateReminderService)
		{
			_dueDateReminderService = dueDateReminderService;
		}

		[HttpGet("ManualCheckDueDates")]  // This will map to the /DueDateTesting/ManualCheckDueDates URL
		public async Task<IActionResult> ManualCheckDueDates()
		{
			await _dueDateReminderService.ManualCheckDueDates();
			return Ok("Manual check of due dates initiated.");
		}
	}
}
