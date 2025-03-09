using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Dashboard;
using _200SXContact.Models.Areas.MaintenApp;

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
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                ILoggerService loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                await loggerService.LogAsync("Due Date Reminder || Manually checking due dates", "Info", "");

                await ProcessDueDates(scope);
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    ILoggerService loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
                    try
                    {
                        DateTime currentTime = DateTime.Now;
                        TimeSpan timeToMidnight = DateTime.Today.AddDays(1) - currentTime;

                        await loggerService.LogAsync($"Due Date Reminder || Time to midnight: {timeToMidnight.TotalMinutes} minutes. Will trigger at {DateTime.Now.Add(timeToMidnight)}", "Info", "");

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
            ILoggerService loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            try
            {
                await loggerService.LogAsync("Due Date Reminder || Started processing due dates", "Info", "");
                List<ReminderItem> dueItems = await context.Items.Where(i => (i.DueDate > DateTime.Now && i.DueDate <= DateTime.Now.AddDays(5)) || (i.DueDate.Date == DateTime.Now.AddDays(10).Date))
                    .Include(i => i.User)
                    .ToListAsync();

                await loggerService.LogAsync($"Due Date Reminder || Found {dueItems.Count} due items", "Info", "");

                foreach (ReminderItem item in dueItems)
                {
                    if (item.User == null || string.IsNullOrEmpty(item.User?.Email))
                    {
                        await loggerService.LogAsync($"Due Date Reminder || Skipping item '{item.EntryItem}' due to invalid user/email.", "Warning", "");

                        continue;
                    }

                    int daysLeft = (item.DueDate - DateTime.Now).Days;
                    await emailService.SendDueDateReminder(item.User.Email, item.User.UserName, item, daysLeft);
                    item.EmailSent = true;

                    await loggerService.LogAsync($"Due Date Reminder || Sent due date reminder for item '{item.EntryItem}' to '{item.User.Email}', due in {daysLeft} days.", "Info", "");
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
