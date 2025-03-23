using _200SXContact.Commands.Areas.UserContent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _200SXContact.Controllers.Areas.UserContent
{
    public class CommentsController : Controller
	{
		private readonly IMediator _mediator;
		public CommentsController(IMediator mediator)
		{
			_mediator = mediator;
		}
		[HttpPost]
		[Route("comments/add-comment")]
		[Authorize]		
		public async Task<IActionResult> PostComment(string userBuildId, string content)
		{
            AddCommentCommand command = new AddCommentCommand
            {
                UserBuildId = userBuildId,
                Content = content,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = User.Identity.Name
            };

            int commentId = await _mediator.Send(command);

			if (commentId == 0)
			{
                TempData["CommentPosted"] = "no";
                TempData["Message"] = "Please enter a comment to submit !";

                return RedirectToAction("DetailedUserView", "UserBuilds", new { id = userBuildId });
            }

            TempData["CommentPosted"] = "yes";
            TempData["Message"] = "Comment posted successfully !";

            return RedirectToAction("DetailedUserView", "UserBuilds", new { id = userBuildId });
        }
		[HttpPost]
		[Route("comments/delete-comment")]
		[Authorize]
		public async Task<IActionResult> DeleteComment(int commentId, string userBuildId)
		{
            DeleteCommentCommand command = new DeleteCommentCommand
            {
                CommentId = commentId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserBuildId = userBuildId
            };

            bool result = await _mediator.Send(command);

            if (!result)
            {
                TempData["CommentDeleted"] = "no";
                TempData["Message"] = "Could not delete comment, please contact us !";

                return RedirectToAction("DetailedUserView", "UserBuilds", new { id = userBuildId });
            }

            TempData["CommentDeleted"] = "yes";
            TempData["Message"] = "Comment deleted !";

            return RedirectToAction("DetailedUserView", "UserBuilds", new { id = userBuildId });
        }
	}
}
