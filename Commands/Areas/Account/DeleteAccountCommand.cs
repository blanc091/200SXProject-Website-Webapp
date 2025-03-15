using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Commands.Areas.Account
{
    public class DeleteAccountCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        public DeleteAccountCommandHandler(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, ILoggerService loggerService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _loggerService = loggerService;
        }
        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Account || Deleting user in profile page (Command Handler)", "Info", "");

            User? user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                await _loggerService.LogAsync("Account || User not found in DeleteAccountCommandHandler", "Error", "");

                return false;
            }

            bool isTokenValid = await _userManager.VerifyUserTokenAsync(user, "Default", "DeleteAccountToken", request.Token);

            if (!isTokenValid)
            {
                await _loggerService.LogAsync("Account || Invalid deletion token", "Error", "");

                return false;
            }

            List<UserBuild> userBuilds = await _context.UserBuilds.Where(ub => ub.UserId == request.UserId).ToListAsync(cancellationToken);

            if (userBuilds.Any())
            {
                await _loggerService.LogAsync($"Account || Found {userBuilds.Count} user build(s) for deletion", "Info", "");

                _context.UserBuilds.RemoveRange(userBuilds);
                await _context.SaveChangesAsync(cancellationToken);

                await _loggerService.LogAsync("Account || Deleted user builds", "Info", "");
            }

            IdentityResult deleteResult = await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                await _loggerService.LogAsync("Account || Failed to delete user", "Error", "");

                foreach (IdentityError error in deleteResult.Errors)
                {
                    await _loggerService.LogAsync($"Account || {error.Description}", "Error", "");
                }

                return false;
            }

            await _signInManager.SignOutAsync();

            await _loggerService.LogAsync("Account || User deleted and signed out", "Info", "");

            return true;
        }
    }
}
