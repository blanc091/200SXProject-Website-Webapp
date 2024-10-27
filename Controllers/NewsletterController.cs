using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _200SXContact.Data;
using _200SXContact.Models;
using System;
using System.Linq;
using System.Net.Mail;
using System.Net;


namespace _200SXContact.Controllers
{
	public class NewsletterController : Controller
	{
		private readonly ApplicationDbContext _context;

		public NewsletterController(ApplicationDbContext context)
		{
			_context = context;
		}
		[Authorize(Roles = "Admin")] 
		public IActionResult CreateNewsletter()
		{
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
                            <img src='https://200sxproject.com/images/verifHeader.JPG' alt='Header Image' />
                        </div>
                        <h1>Account Recovery</h1>
                        <p>Hi there,</p>
                        <p>Click the link below to reset and assign a new password for <b>MaintenApp</b>:</p>
                        <p>
                            <a href='' class='button'>Recover Your Account</a>
                        </p>
                        <p>If you did not request this, you can safely ignore this email.</p>
                        <p>Thank you !</p>
                        <div class='footer'>
                            <p>© 2024 200SX Project. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>"
			};
			
			return View("~/Views/Newsletter/CreateNewsletter.cshtml", model);			
		}
		[HttpPost]
		public IActionResult Subscribe(string email)
		{
			string viewPathSuccess = $"~/Views/Home/Index.cshtml";
			if (string.IsNullOrEmpty(email))
			{
				TempData["IsNewsletterSubscribed"] = "yes";
				TempData["IsNewsletterError"] = "yes";
				TempData["Message"] = "Email required !";
			}

			var existingSubscription = _context.NewsletterSubscriptions
				.FirstOrDefault(sub => sub.Email == email);

			if (existingSubscription != null)
			{
				if (!existingSubscription.IsSubscribed)
				{
					existingSubscription.IsSubscribed = true;
					existingSubscription.SubscribedAt = DateTime.Now;
					_context.SaveChanges();
					TempData["IsNewsletterSubscribed"] = "yes";					
					TempData["Message"] = "Email resubscribed successfully !";
					return View(viewPathSuccess);
				}
				TempData["IsNewsletterSubscribed"] = "yes";
				TempData["IsNewsletterError"] = "yes";
				TempData["Message"] = "Email already registered !";
				return View(viewPathSuccess);
			}

			var subscription = new NewsletterSubscription
			{
				Email = email,
				IsSubscribed = true,
				SubscribedAt = DateTime.Now
			};
			_context.NewsletterSubscriptions.Add(subscription);
			_context.SaveChanges();

			TempData["IsNewsletterSubscribed"] = "yes";
			TempData["Message"] = "Subscribed successfully !";
			return View(viewPathSuccess);
		}

		[HttpPost]
		public IActionResult Unsubscribe(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return BadRequest("Email is required.");
			}

			var subscription = _context.NewsletterSubscriptions
				.FirstOrDefault(sub => sub.Email == email);

			if (subscription == null || !subscription.IsSubscribed)
			{
				return BadRequest("Not subscribed.");
			}

			subscription.IsSubscribed = false;
			_context.SaveChanges();

			return Ok("Unsubscribed successfully.");
		}
		[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
		[HttpPost]
		[Microsoft.AspNetCore.Mvc.ValidateAntiForgeryToken]
		public IActionResult SendNewsletter(NewsletterViewModel model)
		{
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
			return RedirectToAction("CreateNewsletter", "Newsletter");
		}

		[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
		public IActionResult SendNewsletter()
		{
			return View("~/Views/Newsletter/CreateNewsletter.cshtml", new NewsletterViewModel());
		}
		[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
		private void SendEmailToSubscriber(string email, string subject, string body)
		{
			var smtpClient = new SmtpClient("mail5019.site4now.net")
			{
				Port = 587,
				Credentials = new System.Net.NetworkCredential("test@200sxproject.com", "Recall1547!"),
				EnableSsl = true,
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress("test@200sxproject.com"),
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
				Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
			}
		}
	}
}
