using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using _200SXContact.Models;
using NETCore.MailKit.Core;
using PayPalCheckoutSdk.Orders;

namespace _200SXContact.Services
{
	public class DueDateReminderService : IHostedService, IDisposable
	{
		private Timer _timer;
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailService;
		private readonly ILoggerService _loggerService;
		public DueDateReminderService(ApplicationDbContext context, IEmailService emailService, ILoggerService loggerService)
		{
			_context = context;
			_emailService = emailService;
			_loggerService = loggerService;
		}
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var currentTime = DateTime.Now;
            var timeToMidnight = DateTime.Today.AddDays(1) - currentTime;
            _loggerService.LogAsync($"Time to midnight: {timeToMidnight.TotalMinutes} minutes", "Info", "");
            _loggerService.LogAsync($"Setting up timer to trigger at {DateTime.Now.Add(timeToMidnight):HH:mm:ss}", "Info", "");
            _timer = new Timer(ExecuteTimerCallback, null, timeToMidnight, TimeSpan.FromDays(1));
           /* _loggerService.LogAsync("Setting up manual timer every minute for debugging", "Info", "");
            new Timer((state) =>
            {
                _loggerService.LogAsync("Manual timer callback triggered", "Info", "");
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _loggerService.LogAsync("Set up up manual timer every minute for debugging", "Info", "");*/
            return Task.CompletedTask;
        }
        private void ExecuteTimerCallback(object state)
        {
            try
            {
                _loggerService.LogAsync("Timer callback triggered", "Info", "");
                _ = CheckDueDates(state);
            }
            catch (Exception ex)
            {
                _loggerService.LogAsync($"Error during timer callback execution: {ex.Message}", "Error", ex.ToString());
            }
        }
        private async Task CheckDueDates(object? state)
        {
            try
            {
                await _loggerService.LogAsync("CheckDueDates started", "Info", "");
                var dueItems = await _context.Items
                    .Where(i => i.DueDate > DateTime.Now && i.DueDate <= DateTime.Now.AddDays(5) && !i.EmailSent)
                    .Include(i => i.User)
                    .ToListAsync();

                await _loggerService.LogAsync($"Found {dueItems.Count} due items", "Info", "");
                foreach (var item in dueItems)
                {
                    if (item.User == null || string.IsNullOrEmpty(item.User?.Email))
                    {
                        await _loggerService.LogAsync($"Skipping item '{item.EntryItem}' due to invalid user/email.", "Warning", string.Empty);
                        continue;
                    }
                    int daysLeft = (item.DueDate - DateTime.Now).Days;
                    await _emailService.SendDueDateReminder(item.User.Email, item, daysLeft);
                    item.EmailSent = true;

                    await _loggerService.LogAsync($"Sent due date reminder for item '{item.EntryItem}' to '{item.User.Email}', due in {daysLeft} days.", "Info", string.Empty);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Error in due date checking: {ex.Message}", "Error", ex.ToString());
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
		{
			_timer?.Change(Timeout.Infinite, 0);
			_loggerService.LogAsync("Stopped DueDateReminderService","Info","");
            return Task.CompletedTask;
		}
		public void Dispose()
		{
			_timer?.Dispose();
		}
		public async Task ManualCheckDueDates()
		{
			await _loggerService.LogAsync($"Manually checking due dates..", "Error", "");
			await CheckDueDates(null);
		}
	}
}
