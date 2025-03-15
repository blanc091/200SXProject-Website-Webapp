using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;

namespace _200SXContact.Commands.Areas.Account
{
    public class DeleteAccountVerifyCommand : IRequest<bool>
    {
        public required string UserEmail { get; set; }
        public required string ResetUrl { get; set; }
    }
    public class DeleteAccountVerifyCommandHandler : IRequestHandler<DeleteAccountVerifyCommand, bool>
    {
        private readonly ILoggerService _loggerService;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly NetworkCredential _credentials;
        public DeleteAccountVerifyCommandHandler(UserManager<User> userManager, ILoggerService loggerService, IEmailService emailService, NetworkCredential credentials)
        {
            _userManager = userManager;
            _loggerService = loggerService;
            _emailService = emailService;
            _credentials = credentials;
        }
        public async Task<bool> Handle(DeleteAccountVerifyCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync($"Account || Attempting deletion verification for user: {request.UserEmail}", "Info", "");

            User? user = await _userManager.FindByEmailAsync(request.UserEmail);

            if (user == null)
            {
                await _loggerService.LogAsync($"Account || User with email {request.UserEmail} does not exist", "Error", "");

                return false;
            }

            await SendUserDeleteEmail(request.UserEmail, request.ResetUrl);

            await _loggerService.LogAsync($"Account || Deletion email sent to {request.UserEmail}", "Info", "");

            return true;
        }

        private async Task SendUserDeleteEmail(string email, string resetUrl)
        {
            await _loggerService.LogAsync("Account || Starting sending user delete email task", "Info", "");

            MailAddress fromAddress = new MailAddress(_credentials.UserName, "Import Garage");
            MailAddress toAddress = new MailAddress(email);
            string subject = "Import Garage || Delete Your Account";
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
                    <a href=""https://www.200sxproject.com"" target=""_blank"">
					    <img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
				    </a>
                </div>
                <h1>Account Deletion</h1>
                <p>Hi there,</p>
                <p>Click the link below to delete your account and all related info; this action cannot be undone:</p>
                <p>
                    <a href='" + resetUrl + @"' class='button'>Delete Your Account</a>
                </p>
                <p>If you did not request this, you can safely ignore this email.</p>
                <p>Thank you !</p>
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
    }
}
