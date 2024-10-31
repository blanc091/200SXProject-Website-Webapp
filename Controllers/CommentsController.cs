using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _200SXContact.Controllers
{
	public class CommentsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public CommentsController(ApplicationDbContext context)
		{
			_context = context;
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> PostComment(string userBuildId, string content)
		{
			if (string.IsNullOrWhiteSpace(content))
			{
				return BadRequest("Comment content cannot be empty.");
			}

			var comment = new BuildsCommentsModel
			{
				Content = content,
				CreatedAt = DateTime.UtcNow,
				UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
				UserName = User.Identity.Name,
				UserBuildId = userBuildId
			};

			_context.BuildComments.Add(comment);
			await _context.SaveChangesAsync();
			TempData["CommentPosted"] = "yes";
			TempData["Message"] = "Comment posted successfully !";
			return RedirectToAction("DetailedUserView", "UserBuilds", new { id = userBuildId });
		}
	}
}
