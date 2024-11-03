using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public IActionResult AddProduct()
		{
			var model = new Product
			{			
				Name = string.Empty,
				Category = string.Empty,
				Description = string.Empty,
				Price = 0.00m,
				ImagePath = string.Empty,
				DateAdded = DateTime.Now 
			};
			return View("~/Views/Marketplace/AddProduct.cshtml", model);
		}
		[HttpGet]
		public async Task<IActionResult> ProductsDashboard()
		{
			var products = await _context.Products.ToListAsync();
			return View("~/Views/Marketplace/ProductsDashboard.cshtml", products); 
		}
		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddProduct(Product model, IFormFile Image)
		{
			ModelState.Remove("ImagePath");
			if (ModelState.IsValid)
			{
				model.DateAdded = DateTime.Now;

				if (Image != null && Image.Length > 0)
				{
					var productDirectory = Path.Combine("wwwroot/images/products", model.Id.ToString());
					if (!Directory.Exists(productDirectory))
					{
						Directory.CreateDirectory(productDirectory);
					}
					var imagePath = Path.Combine(productDirectory, Image.FileName);
					using (var stream = new FileStream(imagePath, FileMode.Create))
					{
						await Image.CopyToAsync(stream);
					}
					model.ImagePath = $"/images/products/{model.Id}/{Image.FileName}";
				}

				_context.Products.Add(model);
				await _context.SaveChangesAsync();
				return RedirectToAction("ProductsDashboard", "Products");
			}

			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
				_logger.LogError("Model Error: {0}", error.ErrorMessage);
			}
			return View("AddProduct", model);
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
		[HttpGet]
		public async Task<IActionResult> DetailedProductView(int id)
		{
			var product = await _context.Products
					   .FirstOrDefaultAsync(b => b.Id == id);

			if (product == null)
			{
				return NotFound();
			}
			return View("~/Views/Marketplace/DetailedProductView.cshtml", product); 
		}

		public IActionResult OrderSuccess()
		{
			return View();
		}
	}
}
