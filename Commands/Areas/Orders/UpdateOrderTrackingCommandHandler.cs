using _200SXContact.Data;
using _200SXContact.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using AutoMapper;

namespace _200SXContact.Commands.Areas.Orders
{
    public class UpdateOrderTrackingCommandHandler : IRequestHandler<UpdateOrderTrackingCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<UpdateOrderTrackingCommand> _validator;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public UpdateOrderTrackingCommandHandler(ApplicationDbContext context, IValidator<UpdateOrderTrackingCommand> validator, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _validator = validator;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<bool> Handle(UpdateOrderTrackingCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Updating order tracking admin","Info","");

            ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                await _loggerService.LogAsync("Orders || Error updating order tracking admin", "Error", "");

                throw new ValidationException(validationResult.Errors);
            }

            await _loggerService.LogAsync("Orders || Matching Dto to model in order tracking", "Info", "");

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
