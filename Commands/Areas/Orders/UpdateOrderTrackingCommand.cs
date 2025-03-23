using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;

namespace _200SXContact.Commands.Areas.Orders
{
    public class UpdateOrderTrackingCommand(OrderTrackingUpdateDto updateDto) : IRequest<bool>
    {
        public OrderTrackingUpdateDto UpdateDto { get; } = updateDto;
    }
    public class UpdateOrderTrackingCommandHandler : IRequestHandler<UpdateOrderTrackingCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public UpdateOrderTrackingCommandHandler(IApplicationDbContext context, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<bool> Handle(UpdateOrderTrackingCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Updating order tracking admin", "Info", "");

            OrderTrackingUpdateDto updateDto = request.UpdateDto;

            await _loggerService.LogAsync("Orders || Getting order trackings in order tracking admin", "Info", "");

            OrderTracking? orderTracking = await _context.OrderTrackings.FirstOrDefaultAsync(ot => ot.OrderId == updateDto.OrderId, cancellationToken);

            if (orderTracking == null)
            {
                await _loggerService.LogAsync("Orders || Error in order tracking admin, orderTracking is null", "Error", "");

                return false;
            }

            await _loggerService.LogAsync("Orders || Mapping Dto to model in order tracking", "Info", "");

            _mapper.Map(request.UpdateDto, orderTracking);
            orderTracking.StatusUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync("Orders || Mapped Dto to model in order tracking", "Info", "");

            return true;
        }
    }
}
