using _200SXContact.Interfaces.Areas.Admin;
using MediatR;

namespace _200SXContact.Commands.Areas.Account
{
    public class ForgotPasswordCommand : IRequest<ForgotPasswordCommandResult>
    {
        public required string Email { get; set; }
        public required string ResetUrl { get; set; }
    }
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordCommandResult>
    {
        private readonly IEmailService _emailService;
        private readonly ILoggerService _loggerService;
        public ForgotPasswordCommandHandler(IEmailService emailService, ILoggerService loggerService)
        {
            _emailService = emailService;
            _loggerService = loggerService;
        }
        public async Task<ForgotPasswordCommandResult> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            await _emailService.SendPasswordResetEmail(command.Email, command.ResetUrl);

            await _loggerService.LogAsync($"ForgotPassword || Sent reset link to {command.Email}", "Info", "");

            return new ForgotPasswordCommandResult
            {
                Succeeded = true,
                Errors = Array.Empty<string>()
            };
        }
    }
    public class ForgotPasswordCommandResult
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
