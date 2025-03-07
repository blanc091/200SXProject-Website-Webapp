﻿using Microsoft.EntityFrameworkCore;
using AutoMapper;
using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Admin;
using _200SXContact.Models.Areas.Admin;
using System.Net;

namespace _200SXContact.Services
{    
    public class LoggerService : ILoggerService
    {
        private readonly ILoggerService _loggerService;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly NetworkCredential _credentials;
        private readonly IMapper _mapper;
        public LoggerService(ILoggerService loggerService, IMapper mapper, DbContextOptions<ApplicationDbContext> options, ApplicationDbContext context, NetworkCredential credentials)
        {
            _loggerService = loggerService;
            _options = options;
            _context = context;
            _credentials = credentials;
            _mapper = mapper;
        }
        public async Task LogEmailAsync(ContactFormDto model, string status, string errorMessage = null)
        {
            await _loggerService.LogAsync($"Contact form || Logging email with status: {status}", "Info", "");

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

            await _loggerService.LogAsync($"Contact form || Email logged with status: {status}", "Info", "");
        }
        public async Task LogAsync(string message, string logLevel, string exception = "")
        {
            await _loggerService.LogAsync("Contact form || Starting logging email to DB", "Info", "");

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

                await _loggerService.LogAsync("Contact form || Logged contact email to DB", "Info", "");
            }
        }
    }
}
