using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.UserContent;

namespace _200SXContact.Commands.Areas.UserContent
{
    public class SubmitBuildCommand : IRequest<SubmitBuildResult>
    {
        public string UserId { get; set; } = string.Empty;
        public UserBuildDto Model { get; set; }
        public IFormFile[] Images { get; set; }
    }
    public class SubmitBuildCommandHandler : IRequestHandler<SubmitBuildCommand, SubmitBuildResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;
        private readonly IClientTimeProvider _clientTimeProvider;
        public SubmitBuildCommandHandler(IClientTimeProvider clientTimeProvider, IApplicationDbContext context, UserManager<User> userManager, ILoggerService loggerService)
        {
            _context = context;
            _userManager = userManager;
            _loggerService = loggerService;
            _clientTimeProvider = clientTimeProvider;
        }

        public async Task<SubmitBuildResult> Handle(SubmitBuildCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("User builds || Starting to submit user build", "Info", "");

            User? user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                await _loggerService.LogAsync("User builds || User is null when trying to submit user build", "Error", "");

                return SubmitBuildResult.UserNotFound;
            }

            if (string.IsNullOrEmpty(request.Model.Title))
            {
                await _loggerService.LogAsync("User builds || No title entered when trying to submit user build", "Error", "");

                return SubmitBuildResult.NoTitle;
            }
            
            if(string.IsNullOrEmpty(request.Model.Description))
            {
                await _loggerService.LogAsync("User builds || No description entered when trying to submit user build", "Error", "");

                return SubmitBuildResult.NoDescription;
            }

            if (request.Images != null && request.Images.Length > 10)
            {
                await _loggerService.LogAsync("User builds || Too many images uploaded for user build", "Error", "");
                return SubmitBuildResult.TooManyImages;
            }

            DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();

            UserBuild userBuild = new UserBuild
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Model.Title,
                Description = request.Model.Description,
                DateCreated = clientTime,
                UserEmail = user.Email,
                UserName = user.UserName,
                UserId = user.Id
            };

            List<string> imagePaths = new List<string>();

            if (request.Images != null && request.Images.Length > 0)
            {
                await _loggerService.LogAsync("User builds || Submitting images for user build", "Info", "");

                string userDirectory = Path.Combine("wwwroot/images/uploads", user.Id);

                if (!Directory.Exists(userDirectory))
                {
                    Directory.CreateDirectory(userDirectory);
                }

                foreach (IFormFile image in request.Images)
                {
                    if (image.Length == 0)
                    {
                        await _loggerService.LogAsync("User builds || Invalid image when trying to submit user build", "Error", "");

                        return SubmitBuildResult.InvalidImage;
                    }

                    string imagePath = Path.Combine(userDirectory, image.FileName);
                    using (FileStream stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await _loggerService.LogAsync("User builds || Copied image when submitting user build", "Info", "");

                        await image.CopyToAsync(stream, cancellationToken);
                    }
                    imagePaths.Add($"/images/uploads/{user.Id}/{image.FileName}");
                }
            }
            userBuild.ImagePaths = imagePaths;

            await _context.UserBuilds.AddAsync(userBuild, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync($"User builds || Submitted user build for {userBuild.UserId}", "Info", "");

            return SubmitBuildResult.Success;
        }
    }
    public enum SubmitBuildResult
    {
        Success,
        UserNotFound,
        InvalidImage,
        NoTitle,
        NoDescription,
        TooManyImages
    }
}
