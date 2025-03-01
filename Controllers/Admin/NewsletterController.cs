using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _200SXContact.Data;
using _200SXContact.Models;
using System;
using System.Linq;
using System.Net.Mail;
using System.Net;
using _200SXContact.Models.Configs;
using Microsoft.Extensions.Options;
using _200SXContact.Services;

namespace _200SXContact.Controllers.Admin
{
	public class NewsletterController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly NetworkCredential _credentials;
		private readonly IConfiguration _configuration;
		private readonly ILoggerService _loggerService;
		public NewsletterController(ApplicationDbContext context, IOptions<AppSettings> appSettings, IConfiguration configuration, ILoggerService loggerService)
		{
			_context = context;
			var emailSettings = appSettings.Value.EmailSettings;
			_credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
			_configuration = configuration;
            _loggerService = loggerService;
        }
		[HttpGet]
		[Route("newsletter/create-newsletter-admin")]
		[Authorize(Roles = "Admin")]
		public IActionResult CreateNewsletter()
		{
            _loggerService.LogAsync("Newsletter || Getting create newsletter admin page", "Info", "");
            var model = new NewsletterViewModel
			{
				Body = @"<!DOCTYPE html>
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
                            margin: 20px 0;
                        }
                        p {
                            line-height: 1.6;
                            margin: 10px 0;
                            color: #f5f5f5;
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
                        <h1>title of article</h1>
                        <p>Hi there,</p>
                        <p>ADD THE TEXT HEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE</p>
                        <p>
                            <a href='LINK TO ARTICLEEEEEEE' class='button'>Click here to go to the article</a>
                        </p>     
						<p>
							<a href='https://www.200sxproject.com/newsletter/unsubscribe?email={EMAIL}' class='unsubscribe'>Unsubscribe here</a>
						</p>
                        <div class='footer'>
                            <p>© 2024 200SX Project. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>"
			};
            _loggerService.LogAsync("Newsletter || Got create newsletter admin page", "Info", "");
            return View("~/Views/Newsletter/CreateNewsletter.cshtml", model);			
		}
		[HttpPost]
		[Route("newsletter/subscribe")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Subscribe(string email, string honeypotSpam, string gRecaptchaResponseNewsletter)
		{
            await _loggerService.LogAsync("Newsletter || Started subscribing to newsletter for " + email, "Info", "");
            if (!string.IsNullOrWhiteSpace(honeypotSpam))
			{
				return BadRequest("Spam detected");
			}
			if (string.IsNullOrWhiteSpace(gRecaptchaResponseNewsletter) || !await VerifyRecaptchaAsync(gRecaptchaResponseNewsletter))
			{
				TempData["IsNewsletterError"] = "yes";
				TempData["Message"] = "Failed reCAPTCHA validation.";
				return View("~/Views/Home/Index.cshtml");
			}
			if (string.IsNullOrEmpty(email))
			{
                await _loggerService.LogAsync("Newsletter || No email found in request for newsletter subscription", "Error", "");
                TempData["IsNewsletterError"] = "yes";
				TempData["Message"] = "Email required !";
				return View("~/Views/Home/Index.cshtml");
			}
			var existingSubscription = _context.NewsletterSubscriptions
				.FirstOrDefault(sub => sub.Email == email);
			if (existingSubscription != null)
			{
				if (!existingSubscription.IsSubscribed)
				{
					existingSubscription.IsSubscribed = true;
					existingSubscription.SubscribedAt = DateTime.Now;
					await _context.SaveChangesAsync();
					TempData["IsNewsletterSubscribed"] = "yes";
					TempData["IsNewsletterError"] = "no";
					TempData["Message"] = "Email resubscribed successfully !";
                    await _loggerService.LogAsync("Newsletter || Resubscribed for newsletter for " + email, "Info", "");
                    return View("~/Views/Home/Index.cshtml");
				}
				TempData["IsNewsletterError"] = "yes";
				TempData["Message"] = "Email already registered !";
                await _loggerService.LogAsync("Newsletter || Email already registered for newsletter " + email, "Error", "");
                return View("~/Views/Home/Index.cshtml");
			}
			var subscription = new NewsletterSubscription
			{
				Email = email,
				IsSubscribed = true,
				SubscribedAt = DateTime.Now
			};
			await _context.NewsletterSubscriptions.AddAsync(subscription);
			await _context.SaveChangesAsync();
			TempData["IsNewsletterSubscribed"] = "yes";
			TempData["IsNewsletterError"] = "no";
			TempData["Message"] = "Subscribed successfully !";
            await _loggerService.LogAsync("Newsletter || Subscribed to newsletter for " + email, "Info", "");
            return View("~/Views/Home/Index.cshtml");
		}
		private async Task<bool> VerifyRecaptchaAsync(string token)
		{
			var secretKey = _configuration["Recaptcha:SecretKey"];
			using (var client = new HttpClient())
			{
				var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "secret", secretKey },
					{ "response", token }
				});
				var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent);
				if (response.IsSuccessStatusCode)
				{
					var jsonString = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"reCAPTCHA Response: {jsonString}");
					dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
					return result.success == true;
				}
				else
				{
					Console.WriteLine($"Failed to verify reCAPTCHA: {response.StatusCode}");
				}
			}
			return false;
		}
		[HttpGet]
		public IActionResult Unsubscribe(string email)
		{
            _loggerService.LogAsync("Newsletter || Starting unsubscribe request for newsletter subscription", "Info", "");
            if (string.IsNullOrEmpty(email))
			{
                _loggerService.LogAsync("Newsletter || Email not provided or found for newsletter unsubscribe request", "Error", "");
                return BadRequest("Email is required.");
			}
			var subscription = _context.NewsletterSubscriptions
				.FirstOrDefault(sub => sub.Email == email);
			if (subscription == null || !subscription.IsSubscribed)
			{
                _loggerService.LogAsync("Newsletter || Email not subscribed for newsletter and request for unsubscribe sent", "Error", "");
                return BadRequest("Not subscribed.");
			}
			subscription.IsSubscribed = false;
			_context.SaveChangesAsync();
			TempData["Unsubscribed"] = "yes";
			TempData["Message"] = "Unsubscribed from the newsletter !";
            _loggerService.LogAsync("Newsletter || Finished unsubscribe request for newsletter subscription", "Info", "");
            return Redirect("/");
		}
		[HttpPost]
		[Route("newsletter/send-newsletter-admin")]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public IActionResult SendNewsletter(NewsletterViewModel model)
		{
            _loggerService.LogAsync("Newsletter || Started sending newsletter admin", "Info", "");
            if (!ModelState.IsValid)
			{
				return View("~/Views/Newsletter/CreateNewsletter.cshtml", model);
			}
			var subscribers = _context.NewsletterSubscriptions
				.Where(sub => sub.IsSubscribed)
				.Select(sub => sub.Email)
				.ToList();
			foreach (var email in subscribers)
			{
				SendEmailToSubscriber(email, model.Subject, model.Body);
			}
			TempData["Message"] = "Newsletter sent successfully.";
            _loggerService.LogAsync("Newsletter || Finished sending newsletter admin", "Info", "");
            return RedirectToAction("CreateNewsletter", "Newsletter");
		}		
		[Authorize(Roles = "Admin")]
		private void SendEmailToSubscriber(string email, string subject, string body)
		{
            _loggerService.LogAsync("Newsletter || Started sending newsletter email to subscriber admin", "Info", "");
            body = body.Replace("{EMAIL}", WebUtility.UrlEncode(email));
			var smtpClient = new SmtpClient("mail5019.site4now.net")
			{
				Port = 587,
				Credentials = new NetworkCredential(_credentials.UserName, _credentials.Password),
				EnableSsl = true,
			};
			var mailMessage = new MailMessage
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
                _loggerService.LogAsync($"Newsletter || Failed to send email to {email}: {ex.Message}", "Error", "");
			}
            _loggerService.LogAsync("Newsletter || Finished sending newsletter email to subscriber admin", "Info", "");
        }
	}
}
