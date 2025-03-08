using _200SXContact.Models.Areas.Admin;
using System.Net.Mail;
using System.Net;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models;
using System.Text;
using _200SXContact.Interfaces.Areas.Admin;
using System.Text.Json;
using _200SXContact.Models.Configs;
using Microsoft.Extensions.Options;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILoggerService _loggerService;
        private readonly NetworkCredential _credentials;
        private readonly IConfiguration _configuration;
        public EmailService(ILoggerService loggerService, IOptions<AppSettings> appSettings, NetworkCredential credentials, IConfiguration configuration)
        {
            _loggerService = loggerService;
            _configuration = configuration;
            EmailSettings emailSettings = appSettings.Value.EmailSettings;
            _credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
        }
        public async Task SendEmailToAdmin(ContactForm model)
        {
            try
            {
                await _loggerService.LogAsync("Contact form || Started sending contact email to admin", "Info", "");

                MailAddress fromAddress = new MailAddress(_credentials.UserName, model.Email);
                MailAddress toAddress = new MailAddress(_credentials.UserName, "Admin");
                string subject = $"New Contact Form Submission from {model.Name}";
                string body = $"Name: {model.Name}\nEmail: {model.Email}\nMessage: {model.Message}";

                using (SmtpClient smtpClient = new SmtpClient
                {
                    Host = "mail5019.site4now.net",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_credentials.UserName, _credentials.Password)
                })
                {
                    using (MailMessage message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        await _loggerService.LogAsync("Contact form || Email built, sending", "Info", "");

                        await smtpClient.SendMailAsync(message);
                    }
                }

                await _loggerService.LogAsync("Contact form || Sent contact email to admin", "Info", "");
            }
            catch (SmtpException ex)
            {
                string errorMessage = $"SMTP Error: {ex.Message}\n" +
                                   $"StatusCode: {ex.StatusCode}\n" +
                                   $"InnerException: {ex.InnerException?.Message}";

                await _loggerService.LogAsync("Contact form || Failed to send email to the admin. Please try again later" + ex.Message, "Error", "");

                throw new Exception("Failed to send email to the admin. Please try again later", ex);
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync("Contact form || An unexpected error occurred while sending the email" + ex.Message, "Error", "");

                throw new Exception("An unexpected error occurred while sending the email", ex);
            }
        }
        public async Task SendOrderConfirmEmail(string email, OrderPlacement order)
        {
            await _loggerService.LogAsync("Orders || Started sending email with order confirmation", "Info", "");

            MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
            MailAddress toAddress = new MailAddress(email);
            string subject = "Import Garage || Your Order";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='UTF-8'>");
            sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Helvetica', 'Roboto', sans-serif; margin: 0; padding: 0; background-color: #2c2c2c; color: #ece8ed !important; }");
            sb.AppendLine("        .container { width: 100%; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #3c3c3c; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3); }");
            sb.AppendLine("        .header { text-align: center; }");
            sb.AppendLine("        .header img { max-width: 100%; height: auto; border-radius: 8px; }");
            sb.AppendLine("        h1 { color: #ece8ed !important; font-size: 24px; margin: 20px 0; }");
            sb.AppendLine("        p { line-height: 1.6; margin: 10px 0; color: #ece8ed !important; }");
            sb.AppendLine("        .footer { text-align: center; margin-top: 20px; font-size: 12px; color: #b0b0b0; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <a href=\"https://www.200sxproject.com\" target=\"_blank\">\r\n    <img src=\"https://200sxproject.com/images/verifHeader.JPG\" alt=\"200SX Project\" />\r\n</a>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <h1>Order Confirmation</h1>");
            sb.AppendLine("        <p>Hi " + order.FullName + ",</p>");
            if (email == _credentials.UserName)
            {
                sb.AppendLine("        <p>A new order is registered !</p>");
            }
            else
            {
                sb.AppendLine("        <p>Your order has been confirmed !</p>");
            }
            sb.AppendLine("        <p><strong>Order ID:</strong> " + order.Id + "</p>");
            sb.AppendLine("        <p><strong>Order Date:</strong> " + order.OrderDate.ToString("f") + "</p>");
            sb.AppendLine("        <h2 style=\"color: #ece8ed; !important\"><p>Order Details:</p></h2>");
            sb.AppendLine("        <table style='width:100%; border-collapse: collapse;'>");
            sb.AppendLine("            <tr><th style='border: 1px solid #ece8ed !important; padding: 8px;'><p>Item</p></th><th style='border: 1px solid #ece8ed !important; padding: 8px;'><p>Quantity</p></th><th style='border: 1px solid #ece8ed !important; padding: 8px;'><p>Price</p></th></tr>");
            List<CartItem> cartItems = JsonSerializer.Deserialize<List<CartItem>>(order.CartItemsJson) ?? new List<CartItem>();
            foreach (CartItem item in cartItems)
            {
                sb.AppendLine($"<tr><td style='border: 1px solid #ece8ed !important; padding: 8px;'><p>{item.ProductName}<p></td><td style='border: 1px solid #ece8ed !important; padding: 8px;'><p>{item.Quantity}</p></td><td style='border: 1px solid #ece8ed !important; padding: 8px;'><p>{item.Price.ToString("F2")}</p></td></tr>");
            }
            decimal total = cartItems.Sum(item => item.Price * item.Quantity);
            sb.AppendLine($"<tr><td colspan='2' style='border: 1px solid #ece8ed !important; padding: 8px; text-align: right;'><strong><p>Total:</p></strong></td><td style='border: 1px solid #ece8ed !important; padding: 8px;'><p>{total.ToString("F2")}</p></td></tr>");
            sb.AppendLine("        </table>");
            sb.AppendLine("        <p>Thank you !</p>");
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p>© 2024 200SX Project. All rights reserved.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            string body = sb.ToString();
            using (SmtpClient smtpClient = new SmtpClient
            {
                Host = "mail5019.site4now.net",
                Port = 587,
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(_credentials.UserName, _credentials.Password)
            })
            {
                using (MailMessage message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtpClient.SendMailAsync(message);

                    await _loggerService.LogAsync("Orders || Sent email with order confirmation", "Info", "");
                }
            }
        }
        public async Task SendCommentNotification(string userEmail, BuildsComments comment)
        {
            await _loggerService.LogAsync($"Comments || Started sending comment notification for {comment.UserBuildId}", "Info", "");

            string? baseUrl = _configuration["AppSettings:BaseUrl"];
            string link = $"{baseUrl}/detailed-user-build?id={comment.UserBuildId}";
            try
            {
                MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
                MailAddress toAddress = new MailAddress(userEmail);
                string subject = "Import Garage || New comment added for your Build";
                string body = $@"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>
            body {{
                font-family: 'Helvetica', 'Roboto', sans-serif;
                margin: 0;
                padding: 0;
                background-color: #2c2c2c; 
                color: #ffffff; 
            }}
            .container {{
                width: 100%;
                max-width: 600px;
                margin: 0 auto;
                padding: 20px;
                background-color: #3c3c3c;
                border-radius: 8px;
                box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
            }}
            .header {{
                text-align: center;
            }}
            .header img {{
                max-width: 100%;
                height: auto;
                border-radius: 8px;
            }}
            h1 {{
                color: #f5f5f5;
                font-size: 24px;
				font-family: 'Helvetica', 'Roboto', sans-serif;
				margin: 20px 0;
            }}
            p {{
                line-height: 1.6;
                margin: 10px 0;
				color: #f5f5f5;
				font-family: 'Helvetica', 'Roboto', sans-serif;
            }}
            .button {{
                display: inline-block;
                padding: 10px 20px;
                font-size: 16px;
                font-weight: bold;
                color: #ffffff;
                background-color: #d0bed1;
                text-decoration: none;
                border-radius: 5px;
                transition: background-color 0.3s ease;
            }}
            .button:hover {{
                background-color: #966b91; 
            }}
            .footer {{
                text-align: center;
                margin-top: 20px;
                font-size: 12px;
                color: #b0b0b0; 
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <a href=""https://www.200sxproject.com"" target=""_blank"">
					<img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
				</a>
            </div>
            <h1>New comment added</h1>
            <p>Hi there,</p>
			<p>A new comment has been posted on your build:</p>
            <p><blockquote style=""color: #ffffff !important;"">{comment.Content}</blockquote></p>
            <p>
				<a href='{link}' target='_blank'>Click here to access your build page.</a>
			</p>
            <div class='footer'>
                <p>© 2024 200SX Project. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>";
                using (SmtpClient smtpClient = new SmtpClient
                {
                    Host = "mail5019.site4now.net",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential(_credentials.UserName, _credentials.Password)
                })
                {
                    using (MailMessage message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtpClient.SendMailAsync(message);

                        await _loggerService.LogAsync($"Comments || Sent comment notification for build {comment.UserBuildId}", "Info", "");
                    }
                }
            }
            catch (SmtpException ex)
            {
            }
            catch (Exception ex)
            {
            }
        }
        public async Task SendDueDateReminder(string userEmail, string userName, ReminderItem item, int daysBeforeDue)
        {
            try
            {
                await _loggerService.LogAsync($"Due Date Reminder || Started sending item due date notification for item {item}", "Info", "");

                MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
                MailAddress toAddress = new MailAddress(userEmail);
                string subject = $"Import Garage || Reminder: Your service item '{item.EntryItem}' is due in {daysBeforeDue} days !";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html lang='en'>");
                sb.AppendLine("<head>");
                sb.AppendLine("    <meta charset='UTF-8'>");
                sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                sb.AppendLine("    <style>");
                sb.AppendLine("        body { font-family: 'Helvetica', 'Roboto', sans-serif; margin: 0; padding: 0; background-color: #2c2c2c; color: #ece8ed !important; }");
                sb.AppendLine("        .container { width: 100%; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #3c3c3c; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3); }");
                sb.AppendLine("        .header { text-align: center; }");
                sb.AppendLine("        .header img { max-width: 100%; height: auto; border-radius: 8px; }");
                sb.AppendLine("        h1 { color: #ece8ed !important; font-size: 24px; margin: 20px 0; }");
                sb.AppendLine("        p { line-height: 1.6; margin: 10px 0; color: #ece8ed !important; }");
                sb.AppendLine("        .footer { text-align: center; margin-top: 20px; font-size: 12px; color: #b0b0b0; }");
                sb.AppendLine("    </style>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                sb.AppendLine("    <div class='container'>");
                sb.AppendLine("        <div class='header'>");
                sb.AppendLine("            <a href=\"https://www.200sxproject.com\" target=\"_blank\">\r\n    <img src=\"https://200sxproject.com/images/verifHeader.JPG\" alt=\"200SX Project\" />\r\n</a>");
                sb.AppendLine("        </div>");
                sb.AppendLine("        <h1>Your service items</h1>");
                sb.AppendLine("        <p>Hi " + userName + ",</p>");
                sb.AppendLine($"<p>Reminder: Your service item '{item.EntryItem}' is due in {daysBeforeDue} days, on <strong>{item.DueDate.ToString("dd/MM/yyyy")}</strong>.</p>");
                sb.AppendLine("        <h2 style=\"color: #ece8ed; !important\"><p>Please make sure you take the necessary actions. Remember you can log into your profile and check your registered items in MaintenApp dashboard.</p></h2>");
                sb.AppendLine("        <p>Thank you !</p>");
                sb.AppendLine("        <div class='footer'>");
                sb.AppendLine("            <p>© 2024 200SX Project. All rights reserved.</p>");
                sb.AppendLine("        </div>");
                sb.AppendLine("    </div>");
                sb.AppendLine("</body>");
                sb.AppendLine("</html>");
                string body = sb.ToString();
                using (SmtpClient smtpClient = new SmtpClient
                {
                    Host = "mail5019.site4now.net",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential(_credentials.UserName, _credentials.Password)
                })
                {
                    using (MailMessage message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtpClient.SendMailAsync(message);

                        await _loggerService.LogAsync($"Due Date Reminder || Sent item due date notification for item {item}", "Info", "");
                    }
                }
            }
            catch (SmtpException ex)
            {
                string errorMessage = $"SMTP Error: {ex.Message}\n" +
                                   $"StatusCode: {ex.StatusCode}\n" +
                                   $"InnerException: {ex.InnerException?.Message}";

                await _loggerService.LogAsync($"Due Date Reminder || SMTP Error: {ex.Message}", "Error", ex.ToString());

                throw new Exception("Failed to send email for due date reminder. Please try again later.", ex);
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Due Date Reminder || Unexpected Error: {ex.Message}", "Error", ex.ToString());

                throw new Exception("An unexpected error occurred while sending the due date reminder email.", ex);
            }
        }
    }
}
