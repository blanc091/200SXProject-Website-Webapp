﻿using _200SXContact.Helpers;
using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.MaintenApp;
using Microsoft.AspNetCore.Http;

namespace _200SXContact.Commands.Areas.MaintenApp
{
    public class UpdateEntryCommand : IRequest<UpdateEntryResult>
    {
        public int Id { get; set; }
        public required string EntryItem { get; set; }
        public string? EntryDescription { get; set; }
        public DateTime DueDate { get; set; }
    }
    public class UpdateEntryCommandHandler : IRequestHandler<UpdateEntryCommand, UpdateEntryResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IClientTimeProvider _clientTimeProvider;
        public UpdateEntryCommandHandler(IClientTimeProvider clientTimeProvider, IApplicationDbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
            _clientTimeProvider = clientTimeProvider;
        }
        public async Task<UpdateEntryResult> Handle(UpdateEntryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _loggerService.LogAsync("MaintenApp || Started updating entry in MaintenApp dash view", "Info", "");

                ReminderItem? existingItem = await _context.Items.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

                if (existingItem == null)
                {
                    await _loggerService.LogAsync("MaintenApp || Item not found when updating entry", "Error", "");
                    return UpdateEntryResult.ItemNotFound;
                }

                DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();

                existingItem.EntryItem = request.EntryItem;
                existingItem.EntryDescription = request.EntryDescription;
                existingItem.DueDate = request.DueDate;
                existingItem.UpdatedAt = clientTime;

                await _context.SaveChangesAsync(cancellationToken);

                await _loggerService.LogAsync("MaintenApp || Finished updating entry", "Info", "");

                return UpdateEntryResult.Success;
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"MaintenApp || Unknown exception when updating entry: {ex.Message}", "Error", "");

                return UpdateEntryResult.Failure;
            }
        }
    }
    public enum UpdateEntryResult
    {
        Success,
        ItemNotFound,
        Failure
    }
}
