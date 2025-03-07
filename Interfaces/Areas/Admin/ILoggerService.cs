using _200SXContact.Models.DTOs.Areas.Admin;

namespace _200SXContact.Interfaces.Areas.Admin
{
    public interface ILoggerService
    {
        Task LogAsync(string message, string logLevel, string exception = "");
        Task LogEmailAsync(ContactFormDto model, string status, string errorMessage = null);
    }
}
