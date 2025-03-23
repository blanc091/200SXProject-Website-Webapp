using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.MaintenApp;

namespace _200SXContact.Queries.Areas.MaintenApp
{
    public class GetUserDashboardQuery(ClaimsPrincipal user) : IRequest<(GetUserDashboardResult, List<ReminderItemDto>)>
    {
        public ClaimsPrincipal User { get; } = user;
    }
    public class GetUserDashboardQueryHandler : IRequestHandler<GetUserDashboardQuery, (GetUserDashboardResult, List<ReminderItemDto>)>
    {
        private readonly UserManager<User> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public GetUserDashboardQueryHandler(UserManager<User> userManager, IApplicationDbContext context, ILoggerService loggerService, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<(GetUserDashboardResult, List<ReminderItemDto>)> Handle(GetUserDashboardQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("MaintenApp || Started getting MaintenApp dash view", "Info", "");

            if (!request.User.Identity.IsAuthenticated)
            {
                await _loggerService.LogAsync("MaintenApp || User not authenticated when getting MaintenApp dash view", "Error", "");

                return (GetUserDashboardResult.UserNotAuthenticated, new List<ReminderItemDto>());
            }

            User? user = await _userManager.GetUserAsync(request.User);

            if (user == null)
            {
                await _loggerService.LogAsync("MaintenApp || User is null or cannot retrieve user when getting MaintenApp dash view", "Error", "");

                return (GetUserDashboardResult.UserNotFound, new List<ReminderItemDto>());
            }

            User? userWithItems = await _context.Users.Include(u => u.Items).FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (userWithItems == null || !userWithItems.Items.Any())
            {
                await _loggerService.LogAsync("MaintenApp || No items found for the user in MaintenApp dash view", "Info", "");

                return (GetUserDashboardResult.NoItemsFound, new List<ReminderItemDto>());
            }

            List<ReminderItemDto> itemsDto = _mapper.Map<List<ReminderItemDto>>(userWithItems.Items);

            await _loggerService.LogAsync("MaintenApp || Got MaintenApp dash view", "Info", "");

            return (GetUserDashboardResult.Success, itemsDto);
        }
    }
    public enum GetUserDashboardResult
    {
        Success,
        UserNotAuthenticated,
        UserNotFound,
        NoItemsFound
    }    
}
