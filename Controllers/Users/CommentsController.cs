using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _200SXContact.Controllers.Users
{
	public class CommentsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailService;
		private readonly ILoggerService _loggerService;
		public CommentsController(ApplicationDbContext context, IEmailService emailService, ILoggerService loggerService)
		{
			_context = context;
			_emailService = emailService;
			_loggerService = loggerService;
		}
		[HttpPost]
		[Route("comments/add-comment")]
		[Authorize]		
		public async Task<IActionResult> PostComment(string userBuildId, string content)
		{
            await _loggerService.LogAsync("Comments || Starting posting comment for " + userBuildId, "Info", "");
            if (string.IsNullOrWhiteSpace(content))
			{
                await _loggerService.LogAsync("Comments || Submitted empty comment for " + userBuildId, "Error", "");
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
			await _context.BuildComments.AddAsync(comment);
			var userBuild = await _context.UserBuilds.FindAsync(userBuildId);
			if (userBuild != null)
			{
                await _loggerService.LogAsync("Comments || Userbuild is empty when submitting comment", "Error", "");
                var userEmail = userBuild.UserEmail; 													 
				await _emailService.SendCommentNotification(userEmail, comment);
			}
			await _context.SaveChangesAsync();
			TempData["CommentPosted"] = "yes";
			TempData["Message"] = "Comment posted successfully !";
            await _loggerService.LogAsync("Comments || Posted comment for " + userBuildId, "Info", "");
            return RedirectToAction("DetailedUserView", "UserBuilds", new { id = userBuildId });
		}
		[HttpPost]
		[Route("comments/delete-comment")]
		[Authorize]
		public async Task<IActionResult> DeleteComment(int commentId, string userBuildId)
		{
            await _loggerService.LogAsync("Comments || Starting deleting comment for " + userBuildId, "Info", "");
            var comment = await _context.BuildComments.FindAsync(commentId);
			if (comment == null || comment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
			{
                await _loggerService.LogAsync("Comments || Comment or user id is null when trying to delete comment", "Error", "");
                return Forbid();
			}
			_context.BuildComments.Remove(comment);
			await _context.SaveChangesAsync();
            await _loggerService.LogAsync("Comments || Deleted comment for " + userBuildId, "Info", "");
            return RedirectToAction("DetailedUserView", "UserBuilds", new { id = comment.UserBuildId });
		}
	}
}
