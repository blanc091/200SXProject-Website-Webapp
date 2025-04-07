using _200SXContact.Helpers;
using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.UserContent;
using Microsoft.AspNetCore.Http;

namespace _200SXContact.Commands.Areas.UserContent
{
    public class AddCommentCommand : IRequest<int>
    {
        public required string UserBuildId { get; set; }
        public required string Content { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
    }
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IEmailService _emailService;
        private readonly IClientTimeProvider _clientTimeProvider;
        public AddCommentCommandHandler(IClientTimeProvider clientTimeProvider, IApplicationDbContext context, ILoggerService loggerService, IEmailService emailService)
        {
            _context = context;
            _loggerService = loggerService;
            _emailService = emailService;
            _clientTimeProvider = clientTimeProvider;
        }
        public async Task<int> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync($"Comments || Starting posting comment for {request.UserBuildId}", "Info", "");

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                await _loggerService.LogAsync($"Comments || Submitted empty comment for {request.UserBuildId}", "Error", "");

                return 0;
            }

            if (request.Content.Length > 10000)
            {
                await _loggerService.LogAsync($"Comments || Comment is over 10000 characters !", "Error", "");

                return -1;
            }

            DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();

            BuildComment comment = new BuildComment
            {
                Content = request.Content,
                CreatedAt = clientTime,
                UserId = request.UserId,
                UserName = request.UserName,
                UserBuildId = request.UserBuildId
            };

            await _context.BuildComments.AddAsync(comment, cancellationToken);
            UserBuild? userBuild = await _context.UserBuilds.FindAsync([request.UserBuildId], cancellationToken);

            if (userBuild != null)
            {
                string? userEmail = userBuild.UserEmail;
                await _emailService.SendCommentNotification(userEmail, comment);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync($"Comments || Posted comment for {request.UserBuildId}", "Info", "");

            return comment.Id;
        }
    }
}
