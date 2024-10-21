using _200SXContact.Data;
using Microsoft.AspNetCore.Mvc;

namespace _200SXContact.Controllers
{
	public class DetailedViewsController : Controller
	{
		
		public IActionResult DetailedView(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				// Ensure the ID doesn't have spaces, convert it to match the view file name
				var sanitizedId = id.Replace(" ", "-");

				// Return the corresponding view
				return View($"~/Views/DetailedViews/{sanitizedId}.cshtml");
			}
			else
			{
				return RedirectToAction("Index", "Home"); // Redirect to Home if ID is null
			}
		}
	}
}
