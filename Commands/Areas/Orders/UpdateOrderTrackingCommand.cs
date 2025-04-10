using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using static _200SXContact.Commands.Areas.Orders.UpdateOrderTrackingCommandHandler;

namespace _200SXContact.Commands.Areas.Orders
{
    public class UpdateOrderTrackingCommand(OrderTrackingUpdateDto updateDto) : IRequest<UpdateOrderTrackingCommandResult>
    {
        public OrderTrackingUpdateDto UpdateDto { get; } = updateDto;
    }
    public class UpdateOrderTrackingCommandHandler : IRequestHandler<UpdateOrderTrackingCommand, UpdateOrderTrackingCommandResult>
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
        public async Task<UpdateOrderTrackingCommandResult> Handle(UpdateOrderTrackingCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Updating order tracking admin", "Info", "");

            UpdateOrderTrackingCommandValidator validator = new UpdateOrderTrackingCommandValidator();
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errorDictionary = validationResult.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                await _loggerService.LogAsync("Orders || Validation error: " + string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)), "Error", "");
                
                return new UpdateOrderTrackingCommandResult
                {
                    Succeeded = false,
                    Errors = errorDictionary
                };
            }

            OrderTrackingUpdateDto updateDto = request.UpdateDto;

            await _loggerService.LogAsync("Orders || Getting order trackings in order tracking admin", "Info", "");

            OrderTracking? orderTracking = await _context.OrderTrackings.Include(ot => ot.Order).FirstOrDefaultAsync(ot => ot.OrderId == updateDto.OrderId, cancellationToken);

            if (orderTracking == null)
            {
                await _loggerService.LogAsync("Orders || Error in order tracking admin, orderTracking is null", "Error", "");

                return new UpdateOrderTrackingCommandResult
                {
                    Succeeded = false,
                    Errors = new System.Collections.Generic.Dictionary<string, string[]>
                    {
                        { "OrderTracking", new[] { "Order tracking record not found." } }
                    }
                };
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

                return new UpdateOrderTrackingCommandResult
                {
                    Succeeded = false,
                    Errors = new System.Collections.Generic.Dictionary<string, string[]>
                    {
                        { "User", new[] { "User ID not found on associated order." } }
                    }
                };
            }

            string orderUpdateUrl = _urlHelper.Action("UserOrders", "PendingOrders", new { userId }, "https");
            await _emailService.SendOrderUpdateEmail(orderTracking.Email, orderUpdateUrl);

            await _loggerService.LogAsync("Orders || Finished order status update", "Info", "");

            return new UpdateOrderTrackingCommandResult { Succeeded = true };
        }
        public class UpdateOrderTrackingCommandValidator : AbstractValidator<UpdateOrderTrackingCommand>
        {
            public UpdateOrderTrackingCommandValidator()
            {
                RuleFor(x => x.UpdateDto).NotNull().WithMessage("Update data is required !").SetValidator(new OrderTrackingUpdateDtoValidator());
            }
        }
        public class OrderTrackingUpdateDtoValidator : AbstractValidator<OrderTrackingUpdateDto>
        {
            public OrderTrackingUpdateDtoValidator()
            {
                RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required !");

                RuleFor(x => x.Carrier).MaximumLength(100).WithMessage("Carrier cannot exceed 100 characters !");

                RuleFor(x => x.TrackingNumber).MaximumLength(50).WithMessage("Tracking Number cannot exceed 50 characters !");
            }
        }
        public class UpdateOrderTrackingCommandResult
        {
            public bool Succeeded { get; set; }
            public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
        }
    }
}
