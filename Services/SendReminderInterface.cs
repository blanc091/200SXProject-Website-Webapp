using _200SXContact.Models;
using System.Net.Mail;
using System.Threading.Tasks;

namespace _200SXContact.Services
{
	public interface IEmailService
	{
		Task SendDueDateReminder(string userEmail,Item item, int daysBeforeDue);
	}
	public class EmailService : IEmailService
	{
		private readonly ILoggerService _loggerService;
		public EmailService(ILoggerService loggerService)
		{
			_loggerService = loggerService;
		}
		public async Task SendDueDateReminder(string userEmail, Item item, int daysBeforeDue)
		{
			try
			{
				var fromAddress = new MailAddress("test@200sxproject.com", "Admin");
				var toAddress = new MailAddress(userEmail); 
				string subject = $"Reminder: Your service item '{item.EntryItem}' is due in {daysBeforeDue} days";
				string body = $"<p>This is a reminder that your service item '<strong>{item.EntryItem}</strong>' is due on <strong>{item.DueDate.ToShortDateString()}</strong>.</p>";
				using (var smtpClient = new SmtpClient
				{
					Host = "mail5019.site4now.net",
					Port = 587,
					EnableSsl = true,
					Credentials = new System.Net.NetworkCredential("test@200sxproject.com", "Recall1547!")
				})
				{
					using (var message = new MailMessage(fromAddress, toAddress)
					{
						Subject = subject,
						Body = body,
						IsBodyHtml = true
					})
					{
						await smtpClient.SendMailAsync(message);
					}
				}
			}
			catch (SmtpException ex)
			{
				var errorMessage = $"SMTP Error: {ex.Message}\n" +
								   $"StatusCode: {ex.StatusCode}\n" +
								   $"InnerException: {ex.InnerException?.Message}";
				await _loggerService.LogAsync($"SMTP Error: {ex.Message}", "Error");
				throw new Exception("Failed to send email for due date reminder. Please try again later.", ex);
			}
			catch (Exception ex)
			{
				await _loggerService.LogAsync($"Unexpected Error: {ex.Message}", "Error");
				throw new Exception("An unexpected error occurred while sending the due date reminder email.", ex);
			}
		}
	}
}