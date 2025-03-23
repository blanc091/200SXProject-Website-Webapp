using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.Account;
using _200SXContact.Models.DTOs.Areas.UserContent;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Queries.Areas.Account
{
    public class GetUserProfileQuery : IRequest<UserProfileDto?>
    {
        public string UserId { get; set; } = string.Empty;
    }
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public GetUserProfileQueryHandler(IApplicationDbContext context, UserManager<User> userManager, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Account || Getting user profile page (Query Handler)", "Info", "");

            List<UserBuildDto> userBuilds = await _context.UserBuilds
                 .Where(b => b.UserId == request.UserId)
                 .OrderByDescending(b => b.DateCreated)
                 .ProjectTo<UserBuildDto>(_mapper.ConfigurationProvider)
                 .ToListAsync(cancellationToken);

            User? user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                await _loggerService.LogAsync("Account || User is null in profile page (Query Handler)", "Error", "");

                return null;
            }

            UserProfileDto userProfileDto = _mapper.Map<UserProfileDto>(user);
            userProfileDto.UserBuilds = userBuilds;

            await _loggerService.LogAsync("Account || Got user profile page (Query Handler)", "Info", "");

            return userProfileDto;
        }
    }
}
