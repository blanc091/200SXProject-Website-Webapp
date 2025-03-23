using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Commands.Areas.Account
{
    public class ResetPasswordCommand : IRequest<ResetPasswordCommandResult>
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordCommandResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;
        public ResetPasswordCommandHandler(UserManager<User> userManager, ILoggerService loggerService)
        {
            _userManager = userManager;
            _loggerService = loggerService;
        }
        public async Task<ResetPasswordCommandResult> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Login Register || Started ResetPassword Command", "Info", "");

            User? user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
            {
                await _loggerService.LogAsync("Login Register || User is null or invalid email in ResetPassword Command", "Error", "");

                return new ResetPasswordCommandResult
                {
                    Succeeded = false,
                    Errors = new[] { "Invalid token or email." }
                };
            }

            string decodedToken = System.Web.HttpUtility.UrlDecode(command.Token);

            IdentityResult resetResult = await _userManager.ResetPasswordAsync(user, decodedToken, command.NewPassword);

            if (!resetResult.Succeeded)
            {
                string[] errors = resetResult.Errors.Select(e => e.Description).ToArray();
                foreach (string error in errors)
                {
                    await _loggerService.LogAsync("Login Register || Error in ResetPassword Command: " + error, "Error", "");
                }

                return new ResetPasswordCommandResult
                {
                    Succeeded = false,
                    Errors = errors
                };
            }

            await _loggerService.LogAsync("Login Register || Finished ResetPassword Command", "Info", "");

            return new ResetPasswordCommandResult
            {
                Succeeded = true
            };
        }
    }
    public class ResetPasswordCommandResult
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
    }
}
