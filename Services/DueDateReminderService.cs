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
			_timer = new Timer(CheckDueDates, null, TimeSpan.Zero, TimeSpan.FromDays(1));
			return Task.CompletedTask;
		}

		private async void CheckDueDates(object state)
		{
			try
			{
				var dueItems1Day = await _context.Items
					.Where(i => i.DueDate <= DateTime.Now.AddDays(1) && i.DueDate > DateTime.Now && !i.EmailSent)
					.ToListAsync();

				foreach (var item in dueItems1Day)
				{
					await _emailService.SendDueDateReminder(item.User.Email, item, 1);
					item.EmailSent = true;
					await _loggerService.LogAsync($"Sent due date reminder for item '{item.EntryItem}' to '{item.User.Email}' due in 1 day.", "Information"); // Log the info
				}

				var dueItems5Days = await _context.Items
					.Where(i => i.DueDate.Date == DateTime.Now.AddDays(5).Date && !i.EmailSent)
					.ToListAsync();

				foreach (var item in dueItems5Days)
				{
					await _emailService.SendDueDateReminder(item.User.Email, item, 5);
					item.EmailSent = true;
					await _loggerService.LogAsync($"Sent due date reminder for item '{item.EntryItem}' to '{item.User.Email}' due in 5 days.", "Information"); // Log the info
				}
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				await _loggerService.LogAsync($"An error occurred while checking due dates: {ex.Message}", "Error");
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
	}
}
