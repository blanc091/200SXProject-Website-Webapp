using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

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
        private readonly IClientTimeProvider _clientTimeProvider;
        private readonly IEmailService _emailService;
        private readonly IUrlHelper _urlHelper;
        private readonly IActionContextAccessor _actionContextAccessor;
        public UpdateOrderTrackingCommandHandler(IActionContextAccessor actionContextAccessor, IEmailService emailService, IUrlHelperFactory urlHelperFactory, IClientTimeProvider clientTimeProvider, IApplicationDbContext context, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _loggerService = loggerService;
            _mapper = mapper;
            _clientTimeProvider = clientTimeProvider;
            _emailService = emailService;
            _actionContextAccessor = actionContextAccessor;
            _urlHelper = urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        }
        public async Task<bool> Handle(UpdateOrderTrackingCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Updating order tracking admin", "Info", "");

            OrderTrackingUpdateDto updateDto = request.UpdateDto;

            await _loggerService.LogAsync("Orders || Getting order trackings in order tracking admin", "Info", "");

            OrderTracking? orderTracking = await _context.OrderTrackings.Include(ot => ot.Order).FirstOrDefaultAsync(ot => ot.OrderId == updateDto.OrderId, cancellationToken);

            if (orderTracking == null)
            {
                await _loggerService.LogAsync("Orders || Error in order tracking admin, orderTracking is null", "Error", "");

                return false;
            }

            await _loggerService.LogAsync("Orders || Mapping Dto to model in order tracking", "Info", "");

            _mapper.Map(request.UpdateDto, orderTracking);

            DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();

            orderTracking.StatusUpdatedAt = clientTime;
            await _context.SaveChangesAsync(cancellationToken);

            string? userId = orderTracking.Order?.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                await _loggerService.LogAsync("Orders || User ID not found on associated Order", "Error", "");

                return false;
            }

            string orderUpdateUrl = _urlHelper.Action("UserOrders", "PendingOrders", new { userId }, "https");
            await _emailService.SendOrderUpdateEmail(orderTracking.Email, orderUpdateUrl);

            await _loggerService.LogAsync("Orders || Finished order status update", "Info", "");

            return true;
        }
    }
}
