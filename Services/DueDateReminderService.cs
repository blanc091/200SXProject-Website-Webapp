using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using _200SXContact.Models;
using NETCore.MailKit.Core;
using _200SXContact.Interfaces;

namespace _200SXContact.Services
{
    public class DueDateReminderService : BackgroundService, IDueDateReminderService
    {
        private readonly IServiceProvider _serviceProvider;
        public DueDateReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task ManualCheckDueDates()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
                await loggerService.LogAsync("Due Date Reminder || Manually checking due dates", "Info", "");
                await ProcessDueDates(scope);
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
                    try
                    {
                        var currentTime = DateTime.Now;
                        var timeToMidnight = DateTime.Today.AddDays(1) - currentTime;
                        await loggerService.LogAsync(
                            $"Due Date Reminder || Time to midnight: {timeToMidnight.TotalMinutes} minutes. Will trigger at {DateTime.Now.Add(timeToMidnight)}",
                            "Info",
                            "");
                        await Task.Delay(timeToMidnight, stoppingToken);
                        await ProcessDueDates(scope);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        await loggerService.LogAsync($"Due Date Reminder || Unexpected error: {ex.Message}", "Error", ex.ToString());
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    }
                }
            }
        }
        private async Task ProcessDueDates(IServiceScope scope)
        {
            var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            try
            {
                await loggerService.LogAsync("Due Date Reminder || Started processing due dates", "Info", "");
                var dueItems = await context.Items
                    .Where(i =>
                        (i.DueDate > DateTime.Now && i.DueDate <= DateTime.Now.AddDays(5)) ||
                        (i.DueDate.Date == DateTime.Now.AddDays(10).Date))
                    .Include(i => i.User)
                    .ToListAsync();
                await loggerService.LogAsync($"Due Date Reminder || Found {dueItems.Count} due items", "Info", "");
                foreach (var item in dueItems)
                {
                    if (item.User == null || string.IsNullOrEmpty(item.User?.Email))
                    {
                        await loggerService.LogAsync(
                            $"Due Date Reminder || Skipping item '{item.EntryItem}' due to invalid user/email.",
                            "Warning",
                            "");
                        continue;
                    }
                    int daysLeft = (item.DueDate - DateTime.Now).Days;
                    await emailService.SendDueDateReminder(
                        item.User.Email,
                        item.User.UserName,
                        item,
                        daysLeft);
                    item.EmailSent = true;
                    await loggerService.LogAsync(
                        $"Due Date Reminder || Sent due date reminder for item '{item.EntryItem}' to '{item.User.Email}', due in {daysLeft} days.",
                        "Info",
                        "");
                }
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await loggerService.LogAsync($"Due Date Reminder || Error processing due dates: {ex.Message}", "Error", ex.ToString());
            }
        }
    }
}
