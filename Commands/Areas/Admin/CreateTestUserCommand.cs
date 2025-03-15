using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace _200SXContact.Commands.Areas.Admin
{
    public class CreateTestUserCommand : IRequest<bool>
    {
        public string UserName { get; set; } = "testAccount";
        public string Password { get; set; } = "Test@cc34";
    }
    public class CreateTestUserCommandHandler : IRequestHandler<CreateTestUserCommand, bool>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;
        public CreateTestUserCommandHandler(UserManager<User> userManager, ILoggerService loggerService)
        {
            _userManager = userManager;
            _loggerService = loggerService;
        }
        public async Task<bool> Handle(CreateTestUserCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync($"Account || Attempting to create test user: {request.UserName}", "Info", "");

            User? existingUser = await _userManager.FindByNameAsync(request.UserName);

            if (existingUser != null)
            {
                await _loggerService.LogAsync($"Account || Test user '{request.UserName}' already exists", "Error", "");

                return false;
            }

            User testUser = new User
            {
                UserName = request.UserName,
                Email = $"{request.UserName}@example.com",
                EmailConfirmed = true,
                LockoutEnabled = true,
                CreatedAt = DateTime.UtcNow,
                LastLogin = null,
                IsEmailVerified = true
            };

            IdentityResult result = await _userManager.CreateAsync(testUser, request.Password);

            if (result.Succeeded)
            {
                await _loggerService.LogAsync($"Account || Test user '{request.UserName}' created successfully", "Info", "");

                return true;
            }
            else
            {
                await _loggerService.LogAsync($"Account || Failed to create test user '{request.UserName}'", "Error", "");

                foreach (IdentityError error in result.Errors)
                {
                    await _loggerService.LogAsync($"Account || {error.Description}", "Error", "");
                }

                return false;
            }
        }
    }
}
