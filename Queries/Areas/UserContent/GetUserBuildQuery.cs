using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.DTOs.Areas.UserContent;

namespace _200SXContact.Queries.Areas.UserContent
{
    public class GetUserBuildQuery : IRequest<UserBuildDto?>
    {
        public string Id { get; set; } = string.Empty;
    }
    public class GetUserBuildQueryHandler : IRequestHandler<GetUserBuildQuery, UserBuildDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        public GetUserBuildQueryHandler(IApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
        }
        public async Task<UserBuildDto?> Handle(GetUserBuildQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("User builds || Getting detailed user build interface", "Info", "");

            Models.Areas.UserContent.UserBuild? build = await _context.UserBuilds.Include(b => b.Comments).FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (build == null)
            {
                await _loggerService.LogAsync("User builds || Error in DetailedUserView, build not found", "Error", "");

                return null;
            }

            UserBuildDto buildDto = _mapper.Map<UserBuildDto>(build);

            await _loggerService.LogAsync("User builds || Got detailed user build interface", "Info", "");

            return buildDto;
        }
    }
}
