using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Data;
using _200SXContact.Models;

namespace _200SXContact.Services
{
    public interface ILoggerService
    {
        Task LogAsync(string message, string logLevel, string exception);
    }
    public class LoggerService : ILoggerService
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public LoggerService(DbContextOptions<ApplicationDbContext> options)
        {
            _options = options;
        }
        public async Task LogAsync(string message, string logLevel, string exception = "")
        {
            using (var context = new ApplicationDbContext(_options))
            {
                var logEntry = new LoggingModel
                {
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    LogLevel = logLevel,
                    Exception = exception
                };
                await context.Logging.AddAsync(logEntry);
                await context.SaveChangesAsync();
            }
        }
    }
}
