using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using _200SXContact.Models;
using NETCore.MailKit.Core;

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
			_timer = new Timer(ExecuteTimerCallback, null, timeToMidnight, TimeSpan.FromDays(1));
			return Task.CompletedTask;
		}
		private void ExecuteTimerCallback(object state)
		{
			_ = CheckDueDates(state);
		}
		private async Task CheckDueDates(object? state)
		{
			try
			{
				var dueItems = await _context.Items
					.Where(i => i.DueDate > DateTime.Now && i.DueDate <= DateTime.Now.AddDays(5) && !i.EmailSent)
					.Include(i => i.User)
					.ToListAsync();

				foreach (var item in dueItems)
				{
					if (item.User == null)
					{
						await _loggerService.LogAsync($"User for item '{item.EntryItem}' is null, skipping email.", "Warning", string.Empty);
						continue;
					}
					if (string.IsNullOrEmpty(item.User?.Email))
					{
						await _loggerService.LogAsync($"User email is null or empty for item '{item.EntryItem}', skipping email.", "Warning", string.Empty);
						continue;
					}
					int daysLeft = (item.DueDate - DateTime.Now).Days;
					await _emailService.SendDueDateReminder(item.User.Email, item, daysLeft);
					item.EmailSent = true;
					await _loggerService.LogAsync($"Sent due date reminder for item '{item.EntryItem}' to '{item.User.Email}' due in {daysLeft} days.", "Information", string.Empty);
				}
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				await _loggerService.LogAsync($"An error occurred while checking due dates: {ex.Message}", "Error", ex.ToString());
			}
		}
		public Task StopAsync(CancellationToken cancellationToken)
		{
			_timer?.Change(Timeout.Infinite, 0);
			return Task.CompletedTask;
		}
		public void Dispose()
		{
			_timer?.Dispose();
		}
		public async Task ManualCheckDueDates()
		{
			_loggerService.LogAsync($"Manually checking due dates..", "Error", "");
			await CheckDueDates(null);
		}
	}
}
