using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _200SXContact.Controllers
{
	public class CommentsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailService;
		public CommentsController(ApplicationDbContext context, IEmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}
		[HttpPost]
		[Route("comments/add-comment")]
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
			var userBuild = await _context.UserBuilds.FindAsync(userBuildId);
			if (userBuild != null)
			{
				var userEmail = userBuild.UserEmail; 													 
				await _emailService.SendCommentNotification(userEmail, comment);
			}
			await _context.SaveChangesAsync();
			TempData["CommentPosted"] = "yes";
			TempData["Message"] = "Comment posted successfully !";
			return RedirectToAction("DetailedUserView", "UserBuilds", new { id = userBuildId });
		}
		[HttpPost]
		[Route("comments/delete-comment")]
		[Authorize]
		public async Task<IActionResult> DeleteComment(int commentId, string userBuildId)
		{
			var comment = await _context.BuildComments.FindAsync(commentId);
			if (comment == null || comment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
			{
				return Forbid();
			}
			_context.BuildComments.Remove(comment);
			await _context.SaveChangesAsync();
			return RedirectToAction("DetailedUserView", "UserBuilds", new { id = comment.UserBuildId });
		}
	}
}
