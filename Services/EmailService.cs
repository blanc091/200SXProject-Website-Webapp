using _200SXContact.Models.Areas.Admin;
using System.Net.Mail;
using System.Net;
using _200SXContact.Models.Areas.Orders;
using System.Text;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Configs;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.Areas.MaintenApp;

namespace _200SXContact.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILoggerService _loggerService;
        private readonly NetworkCredential _credentials;
        private readonly IConfiguration _configuration;
        private readonly AdminSettings _adminSettings;
        public const string EmailHead = @"
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
</head>";
        public EmailService(ILoggerService loggerService, IOptions<AdminSettings> adminSettings, IOptions<AppSettings> appSettings, NetworkCredential credentials, IConfiguration configuration)
        {
            _loggerService = loggerService;
            _configuration = configuration;
            EmailSettings emailSettings = appSettings.Value.EmailSettings;
            _credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
            _adminSettings = adminSettings.Value;
        }
        [Authorize(Roles = "Admin")]
        public async Task SendEmailToSubscriberAsync(string email, string subject, string body)
        {
            await _loggerService.LogAsync("Newsletter || Started sending newsletter email to subscriber admin", "Info", "");

            body = body.Replace("{EMAIL}", WebUtility.UrlEncode(email));
            SmtpClient smtpClient = new SmtpClient("mail5019.site4now.net")
            {
                Port = 587,
                Credentials = new NetworkCredential(_credentials.UserName, _credentials.Password),
                EnableSsl = true,
            };
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(_credentials.UserName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Newsletter || Failed to send email to {email}: {ex.Message}", "Error", "");
            }

            await _loggerService.LogAsync("Newsletter || Finished sending newsletter email to subscriber admin", "Info", "");
        }
        public async Task NotifyNewChatSessionAsync(string sessionId)
        {
            await _loggerService.LogAsync("Chat box || Sending chat notification to admin", "Info", "");

            using SmtpClient smtpClient = new SmtpClient
            {
                Host = "mail5019.site4now.net",
                Port = 587,
                EnableSsl = true,
                Credentials = _credentials
            };
            string body = EmailHead + $@"
            <body>
                <div class='container'>
                    <div class='header'>
                        <a href=""https://www.200sxproject.com"" target=""_blank"">
                            <img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
                        </a>
                    </div>
                    <h1>New comment added</h1>
                    <p>Hi there,</p>
                    <p>A new chat has been initiated; log in to the admin dash and respond to the users.</p>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(_credentials.UserName, "Admin"),
                Subject = "New Chat Session Started",
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(_adminSettings.Email);

            //await smtpClient.SendMailAsync(mailMessage);

            await _loggerService.LogAsync("Chat box || Sent chat notification to admin", "Info", "");
        }
        public async Task SendOrderUpdateEmailAsync(string email, string orderUpdateUrl)
        {
            await _loggerService.LogAsync("Orders || Starting sending order update email task", "Info", "");

            MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
            MailAddress toAddress = new MailAddress(email);
            string subject = "Import Garage || Order updated";
            string body = EmailHead + $@"
            <body>
                <div class='container'>
                    <div class='header'>
                        <a href=""https://www.200sxproject.com"" target=""_blank"">
                            <img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
                        </a>
                    </div>
                    <h1>Order update</h1>
                    <p>Hi there,</p>
                    <p>Your order has been updated; click the link below to go to your Orders page:</p>
                    <p>
                        <a href='{orderUpdateUrl}' class='button'>My orders</a>
                    </p>
                    <p>Thank you!</p>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
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
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtpClient.SendMailAsync(message);

                    await _loggerService.LogAsync("Orders || Finished sending order update email task", "Info", "");
                }
            }
        }
        public async Task SendUserDeleteEmailAsync(string email, string userDeleteUrl)
        {
            await _loggerService.LogAsync("Account || Starting sending user delete email task", "Info", "");

            MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
            MailAddress toAddress = new MailAddress(email);
            string subject = "Import Garage || Delete Your Account";
            string body = EmailHead + $@"
            <body>
                <div class='container'>
                    <div class='header'>
                        <a href=""https://www.200sxproject.com"" target=""_blank"">
                            <img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
                        </a>
                    </div>
                    <h1>Account Deletion</h1>
                    <p>Hi there,</p>
                    <p>Click the link below to delete your account and all related info; this action cannot be undone:</p>
                    <p>
                        <a href='{userDeleteUrl}' class='button'>Delete Your Account</a>
                    </p>
                    <p>If you did not request this, you can safely ignore this email.</p>
                    <p>Thank you!</p>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
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
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtpClient.SendMailAsync(message);

                    await _loggerService.LogAsync("Account || Finished sending user delete email task", "Info", "");
                }
            }
        }
        public async Task SendPasswordResetEmailAsync(string email, string resetUrl)
        {
            await _loggerService.LogAsync("Login Register || Starting sending password reset email task", "Info", "");

            MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
            MailAddress toAddress = new MailAddress(email);
            string subject = "Import Garage || Reset Your Password";
            string body = EmailHead + $@"
            <body>
            <div class='container'>
                <div class='header'>
                    <a href=""https://www.200sxproject.com"" target=""_blank"">
                        <img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
                    </a>
                </div>
                <h1>Account Recovery</h1>
                <p>Hi there,</p>
                <p>Click the link below to reset and assign a new password for <b>MaintenApp</b>:</p>
                <p>
                    <a href='{resetUrl}' class='button'>Recover Your Account</a>
                </p>
                <p>If you did not request this, you can safely ignore this email.</p>
                <p>Thank you !</p>
                <div class='footer'>
                    <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>
                </div>
            </div>
            </body>
            </html>";
            using (var smtpClient = new SmtpClient
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
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtpClient.SendMailAsync(message);

                    await _loggerService.LogAsync("Login Register || Finished sending password reset email task", "Info", "");
                }
            }
        }
        public async Task SendVerificationEmailAsync(string email, string verificationUrl)
        {
            await _loggerService.LogAsync("Login Register || Starting sending user verification email", "Info", "");

            MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
            MailAddress toAddress = new MailAddress(email);
            string subject = "Import Garage || Verify your email";
            string body = EmailHead + $@"
            <body>
            <div class='container'>
                <div class='header'>
                    <a href=""https://www.200sxproject.com"" target=""_blank"">
                        <img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
                    </a>
                </div>
                <h1>Account Activation</h1>
                <p>Hi there,</p>
                <p>Thank you for registering your account for <b>MaintenApp</b>! To activate your account, please click the button below:</p>
                <p>
                    <a href='{verificationUrl}' class='button'>Activate Your Account</a>
                </p>
                <p>If you did not sign up for this account, you can safely ignore this email.</p>
                <p>Thank you!</p>
                <div class='footer'>
                    <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>
                </div>
            </div>
            </body>
            </html>";
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
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtpClient.SendMailAsync(message);

                    await _loggerService.LogAsync("Login Register || Finished sending user verification email", "Info", "");
                }
            }
        }
        public async Task SendEmailToAdminAsync(ContactForm model)
        {
            try
            {
                await _loggerService.LogAsync("Contact form || Started sending contact email to admin", "Info", "");

                MailAddress fromAddress = new MailAddress(_credentials.UserName, model.Email);
                MailAddress toAddress = new MailAddress(_adminSettings.Email, "Admin");
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
                    using MailMessage message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    };
                    message.ReplyToList.Add(new MailAddress(model.Email));

                    await _loggerService.LogAsync("Contact form || Email built, sending", "Info", "");

                    await smtpClient.SendMailAsync(message);
                }

                await _loggerService.LogAsync("Contact form || Sent contact email to admin", "Info", "");
            }
            catch (SmtpException ex)
            {
                string errorMessage = $"SMTP Error: {ex.Message}\n" + $"StatusCode: {ex.StatusCode}\n" + $"InnerException: {ex.InnerException?.Message}";

                await _loggerService.LogAsync("Contact form || Failed to send email to the admin. Please try again later" + ex.Message, "Error", "");

                throw new Exception("Failed to send email to the admin. Please try again later", ex);
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync("Contact form || An unexpected error occurred while sending the email" + ex.Message, "Error", "");

                throw new Exception("An unexpected error occurred while sending the email", ex);
            }
        }
        public async Task SendOrderConfirmEmailAsync(string email, OrderPlacement order)
        {
            await _loggerService.LogAsync("Orders || Started sending email with order confirmation", "Info", "");

            MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
            MailAddress toAddress = new MailAddress(email);
            string subject = string.Empty;

            if (email == _adminSettings.Email)
            {
                subject = "Import Garage || A new order has been placed !";
            }
            else
            {
                subject = "Import Garage || Your Order";
            }

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

            if (email != _adminSettings.Email)
            {
                sb.AppendLine("        <p>Hi " + order.FullName + ",</p>");
            }

            if (email == _adminSettings.Email)
            {
                sb.AppendLine("        <p>A new order is registered, please log into the admin console and check out the details.</p>");
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
            sb.AppendLine($"            <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            string body = sb.ToString();
            using SmtpClient smtpClient = new SmtpClient
            {
                Host = "mail5019.site4now.net",
                Port = 587,
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(_credentials.UserName, _credentials.Password)
            };
            using MailMessage message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            await smtpClient.SendMailAsync(message);

            await _loggerService.LogAsync("Orders || Sent email with order confirmation", "Info", "");
        }
        public async Task SendCommentNotificationAsync(string userEmail, BuildComment comment)
        {
            await _loggerService.LogAsync($"Comments || Started sending comment notification for {comment.UserBuildId}", "Info", "");

            string? baseUrl = _configuration["AppSettings:BaseUrl"];
            string link = $"{baseUrl}/detailed-user-build?id={comment.UserBuildId}";
            try
            {
                MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
                MailAddress toAddress = new MailAddress(userEmail);
                string subject = "Import Garage || New comment added for your Build";
                string body = EmailHead + $@"                
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
                            <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
                using SmtpClient smtpClient = new SmtpClient
                {
                    Host = "mail5019.site4now.net",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_credentials.UserName, _credentials.Password)
                };
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
            catch (SmtpException ex)
            {
                await _loggerService.LogAsync($"Comments || SMTP error: {ex.ToString()}", "Error", "");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Comments || SMTP error: {ex.ToString()}", "Error", "");
            }
        }
        public async Task SendDueDateReminderAsync(string userEmail, string userName, ReminderItem item, int daysBeforeDue)
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
                sb.AppendLine($"            <p>© {DateTime.Now.Year} 200SX Project. All rights reserved.</p>");
                sb.AppendLine("        </div>");
                sb.AppendLine("    </div>");
                sb.AppendLine("</body>");
                sb.AppendLine("</html>");
                string body = sb.ToString();
                using SmtpClient smtpClient = new SmtpClient
                {
                    Host = "mail5019.site4now.net",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_credentials.UserName, _credentials.Password)
                };
                using MailMessage message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                await smtpClient.SendMailAsync(message);

                await _loggerService.LogAsync($"Due Date Reminder || Sent item due date notification for item {item}", "Info", "");
            }
            catch (SmtpException ex)
            {
                string errorMessage = $"SMTP Error: {ex.Message}\n" + $"StatusCode: {ex.StatusCode}\n" + $"InnerException: {ex.InnerException?.Message}";

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
