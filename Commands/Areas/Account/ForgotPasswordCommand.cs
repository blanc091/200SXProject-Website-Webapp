using _200SXContact.Interfaces.Areas.Admin;

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
            await _loggerService.LogAsync($"ForgotPassword || Starting sending password reset link to {command.Email}", "Info", "");

            ForgotPasswordCommandValidator validator = new ForgotPasswordCommandValidator();
            ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                string[] errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();

                await _loggerService.LogAsync($"ForgotPassword || Validation error: {string.Join(", ", errorMessages)}", "Error", "");

                return new ForgotPasswordCommandResult
                {
                    Succeeded = false,
                    Errors = errorMessages
                };
            }

            await _emailService.SendPasswordResetEmailAsync(command.Email, command.ResetUrl);

            await _loggerService.LogAsync($"ForgotPassword || Sent reset link to {command.Email}", "Info", "");

            return new ForgotPasswordCommandResult
            {
                Succeeded = true,
                Errors = Array.Empty<string>()
            };
        }
    }
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required !").EmailAddress().WithMessage("Please enter a valid email address !").MaximumLength(50).WithMessage("Email cannot exceed 50 characters !");
        }
    }
    public class ForgotPasswordCommandResult
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }    
}
