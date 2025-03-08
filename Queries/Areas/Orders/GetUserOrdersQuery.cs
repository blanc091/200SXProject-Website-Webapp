using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Orders;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace _200SXContact.Queries.Areas.Orders
{
    public class GetUserOrdersQuery(string userId) : IRequest<List<OrderUserDashDto>?>
    {
        public string UserId { get; set; } = userId;
    }
    public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, List<OrderUserDashDto>?>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        public GetUserOrdersQueryHandler(ApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
        }
        public async Task<List<OrderUserDashDto>?> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Getting user orders query", "Info", "");

            List<_200SXContact.Models.Areas.Orders.OrderPlacement> orders = await _context.Orders.Include(o => o.OrderTracking).Where(o => o.UserId == request.UserId).ToListAsync(cancellationToken);

            if (orders == null)
            {
                await _loggerService.LogAsync("Orders || Error when getting user orders query", "Error", "");

                return null;
            }

            List<OrderUserDashDto> orderUserDashDtos = orders.Select(order =>
            {
                _loggerService.LogAsync($"Orders || {order.Id}, CartItemsJson: {order.CartItemsJson}", "Info", "");

                OrderPlacementDto orderPlacementDto = _mapper.Map<OrderPlacementDto>(order);

                _loggerService.LogAsync($"Orders || Mapped OrderPlacementDto: {JsonSerializer.Serialize(orderPlacementDto)}", "Info", "");

                OrderTrackingDto orderTrackingDto = _mapper.Map<OrderTrackingDto>(order.OrderTracking);

                _loggerService.LogAsync($"Orders || Mapped OrderTrackingDto: {JsonSerializer.Serialize(orderTrackingDto)}", "Info", "");

                return new OrderUserDashDto
                {
                    OrderPlacements = new List<OrderPlacementDto> { orderPlacementDto },
                    OrderTrackings = new List<OrderTrackingDto> { orderTrackingDto }
                };
            }).ToList();

            await _loggerService.LogAsync("Orders || Got user orders query", "Info", "");

            return orderUserDashDtos;
        }
    }
}
