using _200SXContact.Models;
using System.Net.Mail;
using System.Threading.Tasks;

namespace _200SXContact.Services
{
	public interface IEmailService
	{
		Task SendDueDateReminder(string userEmail,Item item, int daysBeforeDue);
		Task SendCommentNotification(string userEmail, BuildsCommentsModel comment);
        Task SendOrderConfirmEmail(string email);
	}
	public class EmailService : IEmailService
	{
		private readonly ILoggerService _loggerService;
		public EmailService(ILoggerService loggerService)
		{
			_loggerService = loggerService;
		}
		public async Task SendOrderConfirmEmail(string email) /// to pass checkout model with info
		{
			var fromAddress = new MailAddress("test@200sxproject.com", "Admin");
			var toAddress = new MailAddress(email);
			string subject = "200SX Project || Your Order";
			string body = @"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>
            body {
                font-family: 'Helvetica', 'Roboto', sans-serif;
                margin: 0;
                padding: 0;
                background-color: #2c2c2c; 
                color: #ffffff; 
            }
            .container {
                width: 100%;
                max-width: 600px;
                margin: 0 auto;
                padding: 20px;
                background-color: #3c3c3c;
                border-radius: 8px;
                box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
            }
            .header {
                text-align: center;
            }
            .header img {
                max-width: 100%;
                height: auto;
                border-radius: 8px;
            }
            h1 {
                color: #f5f5f5;
                font-size: 24px;
				font-family: 'Helvetica', 'Roboto', sans-serif;
				margin: 20px 0;
            }
            p {
                line-height: 1.6;
                margin: 10px 0;
				color: #f5f5f5;
				font-family: 'Helvetica', 'Roboto', sans-serif;
            }
            .button {
                display: inline-block;
                padding: 10px 20px;
                font-size: 16px;
                font-weight: bold;
                color: #ffffff;
                background-color: #d0bed1;
                text-decoration: none;
                border-radius: 5px;
                transition: background-color 0.3s ease;
            }
            .button:hover {
                background-color: #966b91; 
            }
            .footer {
                text-align: center;
                margin-top: 20px;
                font-size: 12px;
                color: #b0b0b0; 
            }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <img src='https://200sxproject.com/images/verifHeader.JPG' alt='Header Image' />
            </div>
            <h1>Account Recovery</h1>
            <p>Hi there,</p>
            <p>Your order is confirmed.</p>
            
            <p>If you did not request this, you can safely ignore this email.</p>
            <p>Thank you !</p>
            <div class='footer'>
                <p>© 2024 200SX Project. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>";

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
		public async Task SendCommentNotification(string userEmail, BuildsCommentsModel comment)
		{
			try
			{
				var fromAddress = new MailAddress("test@200sxproject.com", "Admin");
				var toAddress = new MailAddress(userEmail);
				string subject = "New Comment on Your Build";
				string body = @"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>
            body {
                font-family: 'Helvetica', 'Roboto', sans-serif;
                margin: 0;
                padding: 0;
                background-color: #2c2c2c; 
                color: #ffffff; 
            }
            .container {
                width: 100%;
                max-width: 600px;
                margin: 0 auto;
                padding: 20px;
                background-color: #3c3c3c;
                border-radius: 8px;
                box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
            }
            .header {
                text-align: center;
            }
            .header img {
                max-width: 100%;
                height: auto;
                border-radius: 8px;
            }
            h1 {
                color: #f5f5f5;
                font-size: 24px;
				font-family: 'Helvetica', 'Roboto', sans-serif;
				margin: 20px 0;
            }
            p {
                line-height: 1.6;
                margin: 10px 0;
				color: #f5f5f5;
				font-family: 'Helvetica', 'Roboto', sans-serif;
            }
            .button {
                display: inline-block;
                padding: 10px 20px;
                font-size: 16px;
                font-weight: bold;
                color: #ffffff;
                background-color: #d0bed1;
                text-decoration: none;
                border-radius: 5px;
                transition: background-color 0.3s ease;
            }
            .button:hover {
                background-color: #966b91; 
            }
            .footer {
                text-align: center;
                margin-top: 20px;
                font-size: 12px;
                color: #b0b0b0; 
            }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <img src='https://200sxproject.com/images/verifHeader.JPG' alt='Header Image' />
            </div>
            <h1>Account Activation</h1>
            <p>Hi there,</p>
			<p>A new comment has been posted on your build:</p>
            <P><blockquote>{comment.Content}</blockquote></p>
            
            <p>TO IMPLEMENT TTTTTTTTTTTTTTTTTTT CLICK HERE TO SEE</p>
            <div class='footer'>
                <p>© 2024 200SX Project. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>";
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
				// Handle SMTP 
			}
			catch (Exception ex)
			{
				// Handle other exception
			}
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