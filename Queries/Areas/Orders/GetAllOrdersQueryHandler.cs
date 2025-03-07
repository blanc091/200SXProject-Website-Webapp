using _200SXContact.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using _200SXContact.Interfaces.Areas.Admin;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderTrackingUpdateDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILoggerService _loggerService;
    public GetAllOrdersQueryHandler(ApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
    {
        _context = context;
        _mapper = mapper;
        _loggerService = loggerService;
    }
    public async Task<List<OrderTrackingUpdateDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        await _loggerService.LogAsync("Orders || Get all orders Admin","Info","");

        List<OrderPlacement> orders = await _context.Orders.ToListAsync(cancellationToken);

        await _loggerService.LogAsync("Orders || Get all order trackings Admin", "Info", "");

        List<OrderTracking> orderTrackings = await _context.OrderTrackings.ToListAsync(cancellationToken);

        await _loggerService.LogAsync("Orders || Building orderTrackingDtos Admin", "Info", "");

        List<OrderTrackingDto> orderTrackingDtos = orders.Select(order =>
        {
            OrderTrackingDto dto = _mapper.Map<OrderTrackingDto>(order);
            OrderTracking? orderTracking = orderTrackings.FirstOrDefault(ot => ot.OrderId == order.Id);

            dto.Status = orderTracking?.Status ?? "Unknown";
            dto.AddressLine = $"{order.AddressLine1} {order.AddressLine2}";
            dto.CartItemsJson = order.CartItemsJson;

            return dto;
        }).ToList();

        List<OrderTrackingUpdateDto> orderTrackingUpdateDtos = _mapper.Map<List<OrderTrackingUpdateDto>>(orderTrackingDtos);

        await _loggerService.LogAsync("Orders || Got all orders Admin", "Info", "");

        return orderTrackingUpdateDtos;
    }
}
