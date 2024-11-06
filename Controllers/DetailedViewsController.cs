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
				var sanitizedId = id.Replace(" ", "-");
				return View($"~/Views/DetailedViews/{sanitizedId}.cshtml");
			}
			else
			{
				return RedirectToAction("Index", "Home"); 
			}
		}
	}
}
