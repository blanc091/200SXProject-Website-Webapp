using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.Orders;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetOrderSummaryQuery(int orderId) : IRequest<OrderUserDashDto?>
    {
        public int OrderId { get; } = orderId;
    }
    public class GetOrderSummaryQueryHandler : IRequestHandler<GetOrderSummaryQuery, OrderUserDashDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GetOrderSummaryQueryHandler(IApplicationDbContext context, IMapper mapper, ILoggerService loggerService, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<OrderUserDashDto?> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Getting order summary after order complete", "Info", "");

            string? userId = _userManager.GetUserId(_httpContextAccessor.HttpContext?.User);
            Models.Areas.Orders.OrderPlacement? order = await _context.Orders.FirstOrDefaultAsync(c => c.Id == request.OrderId, cancellationToken);

            if (order == null || order.UserId != userId)
            {
                await _loggerService.LogAsync("Orders || Order or user are null in order summary after order complete", "Error", "");

                return null;
            }

            await _loggerService.LogAsync("Orders || Getting orderPlacementDto and orderTrackingDto", "Error", "");

            OrderPlacementDto orderPlacementDto = _mapper.Map<OrderPlacementDto>(order);
            orderPlacementDto.CartItemsJson = order.CartItemsJson;

            Models.Areas.Orders.OrderTracking? orderTracking = await _context.OrderTrackings.FirstOrDefaultAsync(ot => ot.OrderId == order.Id, cancellationToken);
            OrderTrackingDto orderTrackingDto = _mapper.Map<OrderTrackingDto>(orderTracking);

            await _loggerService.LogAsync("Orders || Got orderPlacementDto and orderTrackingDto", "Error", "");

            await _loggerService.LogAsync("Orders || Building orderUserDashDto", "Info", "");

            OrderUserDashDto orderUserDashDto = new OrderUserDashDto
            {
                OrderPlacements = new List<OrderPlacementDto> { orderPlacementDto },
                OrderTrackings = new List<OrderTrackingDto> { orderTrackingDto }
            };

            await _loggerService.LogAsync("Orders || Got order summary after order complete", "Info", "");

            return orderUserDashDto;
        }
    }
}
