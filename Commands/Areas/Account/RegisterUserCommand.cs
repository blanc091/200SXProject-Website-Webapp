using _200SXContact.Commands.Areas.Newsletter;
using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Commands.Areas.Account
{
    public class RegisterUserCommand : IRequest<RegisterUserCommandResult>
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool SubscribeToNewsletter { get; set; }
        public string? HoneypotSpam { get; set; }
        public required string RecaptchaResponse { get; set; }
        public required string TimeZoneCookie { get; set; }
        public required string VerificationUrl { get; set; }
        public required string VerificationToken { get; set; }
        public required bool IsCalledFromRegisterForm { get; set; }
    }
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserCommandResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;
        private readonly IEmailService _emailService;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IClientTimeProvider _clientTimeProvider;
        public RegisterUserCommandHandler(IClientTimeProvider clientTimeProvider, UserManager<User> userManager, ILoggerService loggerService, IEmailService emailService, IMediator mediator, IConfiguration configuration)
        {
            _userManager = userManager;
            _loggerService = loggerService;
            _emailService = emailService;
            _mediator = mediator;
            _configuration = configuration;
            _clientTimeProvider = clientTimeProvider;
        }
        public async Task<RegisterUserCommandResult> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Login Register || Starting new user registration process", "Info", "");

            if (!string.IsNullOrWhiteSpace(command.HoneypotSpam))
            {
                return new RegisterUserCommandResult
                {
                    Succeeded = false,
                    Errors = ["Spam detected."]
                };
            }

            if (string.IsNullOrWhiteSpace(command.RecaptchaResponse) || !await VerifyRecaptchaAsync(command.RecaptchaResponse))
            {
                return new RegisterUserCommandResult
                {
                    Succeeded = false,
                    Errors = ["Failed reCAPTCHA validation."]
                };
            }

            User? existingUser = await _userManager.FindByEmailAsync(command.Email) ?? await _userManager.FindByNameAsync(command.Username);

            if (existingUser != null)
            {
                if (command.IsCalledFromRegisterForm)
                {
                    await _loggerService.LogAsync("User already exists when trying to register new user", "Error", "");

                    return new RegisterUserCommandResult
                    {
                        Succeeded = false,
                        Errors = ["User already exists!"]
                    };
                }
                else
                {
                    if (existingUser.IsEmailVerified)
                    {
                        return new RegisterUserCommandResult
                        {
                            Succeeded = false,
                            Errors = ["User already activated."]
                        };
                    }
                    else
                    {
                        existingUser.EmailVerificationToken = command.VerificationToken;
                        await _userManager.UpdateAsync(existingUser);
                        await _emailService.SendVerificationEmail(existingUser.Email, command.VerificationUrl);

                        return new RegisterUserCommandResult { Succeeded = true, Errors = [] };
                    }
                }
            }

            DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();
            DateTime createdAt = clientTime;

            User user = new()
            {
                UserName = command.Username,
                Email = command.Email,
                CreatedAt = createdAt,
                IsEmailVerified = false,
                EmailVerificationToken = command.VerificationToken
            };

            IdentityResult createResult = await _userManager.CreateAsync(user, command.Password);

            if (!createResult.Succeeded)
            {
                string[] errors = createResult.Errors.Select(e => e.Description).ToArray();
                foreach (string error in errors)
                {
                    await _loggerService.LogAsync($"Login Register || Error in user creation: {error}", "Error", "");
                }

                return new RegisterUserCommandResult { Succeeded = false, Errors = errors };
            }

            if (command.SubscribeToNewsletter)
            {
                await _loggerService.LogAsync("Login Register || New user opted for newsletter registration", "Info", "");

                SubscribeToNewsletterCommand subscribeCommand = new SubscribeToNewsletterCommand
                {
                    Email = command.Email,
                    HoneypotSpam = command.HoneypotSpam,
                    RecaptchaResponse = command.RecaptchaResponse,
                    IsCalledFromRegisterForm = true
                };

                await _mediator.Send(subscribeCommand);
            }

            await _emailService.SendVerificationEmail(command.Email, command.VerificationUrl);

            await _loggerService.LogAsync("Login Register || New user registered, redirecting to login page", "Info", "");

            return new RegisterUserCommandResult { Succeeded = true, Errors = [] };
        }
        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            string? secretKey = _configuration["Recaptcha:SecretKey"];
            using (HttpClient client = new HttpClient())
            {
                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", token }
                });
                HttpResponseMessage response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

                    return result.success == true;
                }
                else
                {
                    Console.WriteLine($"Failed to verify reCAPTCHA: {response.StatusCode}");
                }
            }

            return false;
        }
    }
    public class RegisterUserCommandResult
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
