using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.DTOs.Areas.UserContent;

namespace _200SXContact.Queries.Areas.UserContent
{
    public class GetUserBuildsQuery : IRequest<List<UserBuildDto>> { }
    public class GetUserBuildsQueryHandler : IRequestHandler<GetUserBuildsQuery, List<UserBuildDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        public GetUserBuildsQueryHandler(IApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
        }
        public async Task<List<UserBuildDto>> Handle(GetUserBuildsQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("User builds || Getting user builds dashboard", "Info", "");

            List<Models.Areas.UserContent.UserBuild> builds = await _context.UserBuilds.OrderByDescending(b => b.DateCreated).ToListAsync(cancellationToken);

            List<UserBuildDto> buildDtos = _mapper.Map<List<UserBuildDto>>(builds);

            await _loggerService.LogAsync("User builds || Got user builds dashboard", "Info", "");

            return buildDtos;
        }
    }
}
