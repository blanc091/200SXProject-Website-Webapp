using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using _200SXContact.Models;

namespace _200SXContact.Services
{
	public interface ILoggerService
	{
		Task LogAsync(string message, string logLevel);
	}
	public class LoggerService : ILoggerService
	{
		private readonly ApplicationDbContext _context;

		public LoggerService(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task LogAsync(string message, string logLevel)
		{
			var logEntry = new LoggingModel
			{
				Message = message,
				Timestamp = DateTime.UtcNow,
				LogLevel = logLevel
			};
			_context.Logging.Add(logEntry);
			await _context.SaveChangesAsync();
		}
	}
}
