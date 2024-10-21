using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;

namespace _200SXContact.Controllers
{
	public class NewsletterController : Microsoft.AspNetCore.Mvc.Controller
	{
		private readonly ApplicationDbContext _context;

		public NewsletterController(ApplicationDbContext context)
		{
			_context = context;
		}
		[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")] 
		public IActionResult CreateNewsletter()
		{
			return View("~/Views/Newsletter/CreateNewsletter.cshtml");
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
			return View(new NewsletterViewModel());
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
