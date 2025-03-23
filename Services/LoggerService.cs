using Microsoft.EntityFrameworkCore;
using AutoMapper;
using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;
using _200SXContact.Models.Areas.Admin;
using System.Net;
using _200SXContact.Interfaces.Areas.Data;

namespace _200SXContact.Services
{    
    public class LoggerService : ILoggerService
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly IApplicationDbContext _context;
        private readonly NetworkCredential _credentials;
        private readonly IMapper _mapper;
        public LoggerService(IMapper mapper, DbContextOptions<ApplicationDbContext> options, IApplicationDbContext context, NetworkCredential credentials)
        {
            _options = options;
            _context = context;
            _credentials = credentials;
            _mapper = mapper;
        }
        public async Task LogEmailAsync(ContactFormDto model, string status, string errorMessage = null)
        {
            EmailLogDto emailLogDto = new EmailLogDto
            {
                Timestamp = DateTime.Now,
                From = model.Email,
                To = _credentials.UserName,
                Subject = $"New Contact Form Submission from {model.Name}",
                Body = model.Message,
                Status = status,
                ErrorMessage = errorMessage
            };

            EmailLog emailLog = _mapper.Map<EmailLog>(emailLogDto);

            await _context.EmailLogs.AddAsync(emailLog);
            await _context.SaveChangesAsync();
        }
        public async Task LogAsync(string message, string logLevel, string exception = "")
        {
            using (ApplicationDbContext context = new ApplicationDbContext(_options))
            {
                LoggingDto logEntryDto = new LoggingDto
                {
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    LogLevel = logLevel,
                    Exception = exception
                };

                Logging logging = _mapper.Map<Logging>(logEntryDto);

                await context.Logging.AddAsync(logging);
                await context.SaveChangesAsync();
            }
        }
    }
}
