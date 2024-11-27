using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace _200SXContact.Services
{
	public class DueDateReminderServiceHost : IHostedService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		public DueDateReminderServiceHost(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var dueDateReminderService = scope.ServiceProvider.GetRequiredService<DueDateReminderService>();
				await dueDateReminderService.StartAsync(cancellationToken);
			}
		}
		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
