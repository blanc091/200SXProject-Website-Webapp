using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace _200SXContact.Commands.Areas.Account
{
    public class DeleteAccountVerifyCommand : IRequest<bool>
    {
        public required string UserEmail { get; set; }
        public required string ResetUrl { get; set; }
    }
    public class DeleteAccountVerifyCommandHandler : IRequestHandler<DeleteAccountVerifyCommand, bool>
    {
        private readonly ILoggerService _loggerService;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;      
        public DeleteAccountVerifyCommandHandler(UserManager<User> userManager, ILoggerService loggerService, IEmailService emailService)
        {
            _userManager = userManager;
            _loggerService = loggerService;
            _emailService = emailService;
        }
        public async Task<bool> Handle(DeleteAccountVerifyCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync($"Account || Attempting deletion verification for user: {request.UserEmail}", "Info", "");

            User? user = await _userManager.FindByEmailAsync(request.UserEmail);

            if (user == null)
            {
                await _loggerService.LogAsync($"Account || User with email {request.UserEmail} does not exist", "Error", "");

                return false;
            }

            await _emailService.SendUserDeleteEmail(request.UserEmail, request.ResetUrl);

            await _loggerService.LogAsync($"Account || Deletion email sent to {request.UserEmail}", "Info", "");

            return true;
        }        
    }
}
