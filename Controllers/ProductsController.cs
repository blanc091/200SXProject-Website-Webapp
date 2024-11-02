using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace _200SXContact.Controllers
{
	public class ProductsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<LoginRegisterController> _logger;
		private readonly UserManager<User> _userManager;
		public ProductsController(ApplicationDbContext context, ILogger<LoginRegisterController> logger, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
			_logger = logger;
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> PlaceOrder(int productId)
		{
			var product = await _context.Products.FindAsync(productId);

			if (product == null)
			{
				return NotFound("Product not found.");
			}

			var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
			{
				return BadRequest("User email not found.");
			}

			var subject = "Order Confirmation";
			var body = $"<p>Thank you for ordering <strong>{product.Name}</strong>!</p>" + //// to get hmtl body and adjust
					   $"<p>Your order has been received and will be processed soon.</p>" +
					   $"<p><strong>Order Details:</strong></p>" +
					   $"<ul><li>Product: {product.Name}</li><li>Price: ${product.Price}</li></ul>";

			SendConfirmationEmail(userEmail, subject, body);

			return RedirectToAction("OrderSuccess");
		}
		private void SendConfirmationEmail(string userEmail, string subject, string body)
		{
			using (var smtpClient = new SmtpClient("mail5019.site4now.net")
			{
				Port = 587,
				Credentials = new System.Net.NetworkCredential("test@200sxproject.com", "Recall1547!"),
				EnableSsl = true,
			})
			{
				var userMailMessage = new MailMessage
				{
					From = new MailAddress("test@200sxproject.com"),
					Subject = subject,
					Body = body,
					IsBodyHtml = true,
				};
				userMailMessage.To.Add(userEmail);

				var adminMailMessage = new MailMessage
				{
					From = new MailAddress("test@200sxproject.com"),
					Subject = "New Order Received",
					Body = $"A new order has been placed by {userEmail}. Please check the details in the order management system.",
					IsBodyHtml = true,
				};
				adminMailMessage.To.Add("mircea.albu91@gmail.com");

				try
				{
					smtpClient.Send(userMailMessage);
					smtpClient.Send(adminMailMessage);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to send email: {ex.Message}");
				}
			}
		}
		public IActionResult OrderSuccess()
		{
			return View();
		}
	}
}
