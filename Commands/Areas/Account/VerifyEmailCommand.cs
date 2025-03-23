using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Commands.Areas.Account
{
    public class VerifyEmailCommand : IRequest<VerifyEmailCommandResult>
    {
        public string? Token { get; set; }
    }
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailCommandResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;
        public VerifyEmailCommandHandler(UserManager<User> userManager, ILoggerService loggerService)
        {
            _userManager = userManager;
            _loggerService = loggerService;
        }
        public async Task<VerifyEmailCommandResult> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Login Register || Starting VerifyEmail Command", "Info", "");

            User? user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == command.Token, cancellationToken);

            if (user == null)
            {
                await _loggerService.LogAsync("Login Register || User is null or invalid token in VerifyEmail Command", "Error", "");

                return new VerifyEmailCommandResult
                {
                    Succeeded = false,
                    Error = "Invalid token."
                };
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _loggerService.LogAsync("Login Register || Finished VerifyEmail Command", "Info", "");

            return new VerifyEmailCommandResult
            {
                Succeeded = true
            };
        }
    }
    public class VerifyEmailCommandResult
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
    }
}
