using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using MediatR;

namespace _200SXContact.Commands.Areas.UserContent
{
    public class DeleteCommentCommand : IRequest<bool>
    {
        public int CommentId { get; set; }
        public required string UserId { get; set; }
        public required string UserBuildId { get; set; }
    }
    public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        public DeleteCommentHandler(ApplicationDbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
        }
        public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync($"Comments || Starting deleting comment for {request.UserBuildId}", "Info", "");

            Models.Areas.UserContent.BuildComment? comment = await _context.BuildComments.FindAsync([request.CommentId], cancellationToken);

            if (comment == null || comment.UserId != request.UserId)
            {
                await _loggerService.LogAsync("Comments || Comment or user ID is null when trying to delete comment", "Error", "");

                return false;
            }

            _context.BuildComments.Remove(comment);
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync($"Comments || Deleted comment for {request.UserBuildId}", "Info", "");

            return true;
        }
    }
}
