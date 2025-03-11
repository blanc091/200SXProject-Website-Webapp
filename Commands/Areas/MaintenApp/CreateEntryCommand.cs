using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.MaintenApp;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace _200SXContact.Commands.Areas.MaintenApp
{
    public class CreateEntryCommand(ClaimsPrincipal user, ReminderItemDto entryDto) : IRequest<CreateEntryResult>
    {
        public ClaimsPrincipal User { get; } = user;
        public ReminderItemDto EntryDto { get; } = entryDto;
    }
    public class CreateEntryCommandHandler : IRequestHandler<CreateEntryCommand, CreateEntryResult>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;

        public CreateEntryCommandHandler(ApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
        }

        public async Task<CreateEntryResult> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("MaintenApp || Started adding entry in MaintenApp dash view", "Info", "");

            string? userEmail = request.User.FindFirstValue(ClaimTypes.Email);
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

            if (user == null)
            {
                await _loggerService.LogAsync("MaintenApp || User is null when creating entry in MaintenApp dash view", "Error", "");

                return CreateEntryResult.UserNotFound;
            }

            if (request.EntryDto.DueDate == default)
            {
                await _loggerService.LogAsync("MaintenApp || DueDate is default value when creating user entry", "Error", "");

                return CreateEntryResult.InvalidDueDate;
            }

            ReminderItem newItem = _mapper.Map<ReminderItem>(request.EntryDto);
            newItem.DueDate = request.EntryDto.DueDate;
            newItem.CreatedAt = DateTime.Now;
            newItem.UpdatedAt = DateTime.Now;
            newItem.UserId = user.Id;

            await _context.Items.AddAsync(newItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync("MaintenApp || Finished adding entry in MaintenApp dash view", "Info", "");

            return CreateEntryResult.Success;
        }
    }
    public enum CreateEntryResult
    {
        Success,
        UserNotFound,
        InvalidDueDate
    }
}
