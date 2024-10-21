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

		public DueDateReminderService(ApplicationDbContext context, IEmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_timer = new Timer(CheckDueDates, null, TimeSpan.Zero, TimeSpan.FromDays(1)); 
			return Task.CompletedTask;
		}

		private async void CheckDueDates(object state)
		{
			/*var dueItems10Days = await _context.Items
				.Where(i => i.DueDate <= DateTime.Now.AddDays(10) && i.DueDate > DateTime.Now.AddDays(5))
				.ToListAsync();*/
			var dueItems1Day = await _context.Items
	.Where(i => i.DueDate <= DateTime.Now.AddDays(1) && i.DueDate > DateTime.Now)
	.ToListAsync();
			foreach (var item in dueItems1Day)
			{
				await _emailService.SendDueDateReminder(item.User.Email, item, 10);
			}

			var dueItems5Days = await _context.Items
				.Where(i => i.DueDate.Date == DateTime.Now.AddDays(5).Date)
				.ToListAsync();

			foreach (var item in dueItems5Days)
			{
				await _emailService.SendDueDateReminder(item.User.Email, item, 5);
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