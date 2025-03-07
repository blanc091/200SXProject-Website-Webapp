﻿using _200SXContact.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using AutoMapper;
using _200SXContact.Interfaces.Areas.Admin;

namespace _200SXContact.Commands.Areas.Orders
{
    public class UpdateOrderTrackingCommandHandler : IRequestHandler<UpdateOrderTrackingCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public UpdateOrderTrackingCommandHandler(ApplicationDbContext context, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<bool> Handle(UpdateOrderTrackingCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Orders || Updating order tracking admin","Info","");

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
