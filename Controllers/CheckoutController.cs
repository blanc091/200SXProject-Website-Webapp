using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using _200SXContact.Services;
using _200SXContact.Models.Configs;

namespace _200SXContact.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly Services.IEmailService _emailService;
		private readonly AdminSettings _adminSettings;
		public CheckoutController(ApplicationDbContext context, UserManager<User> userManager, Services.IEmailService emailService, AdminSettings adminSettings)
		{
			_emailService = emailService;
			_context = context;
			_userManager = userManager;
			_adminSettings = adminSettings;
		}
		[HttpGet]
		public IActionResult Checkout()
		{
			return View("~/Views/Marketplace/CheckoutView.cshtml");
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> PlaceOrder(Order model)
		{
			ModelState.Remove("UserId");
			ModelState.Remove("User");
			ModelState.Remove("CartItems");
			ModelState.Remove("OrderItems");
			if (!ModelState.IsValid)
			{
				return View("~/Views/Marketplace/CheckoutView.cshtml", model);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				TempData["IsUserLoggedIn"] = "no";
				TempData["Message"] = "You need to be registered and logged in to checkout.";
				return RedirectToAction("Login", "LoginRegister");
			}

			model.UserId = user.Id;
			var cartItems = await _context.CartItems.Where(ci => ci.UserId == user.Id).ToListAsync();
			model.CartItems = cartItems;
			model.OrderItems = new List<OrderItem>();

			foreach (var cartItem in cartItems)
			{
				model.OrderItems.Add(new OrderItem
				{
					CartItemId = cartItem.Id,
					Quantity = cartItem.Quantity,
					Price = cartItem.Price, 
					ProductName = cartItem.ProductName 
				});
			}
			
			_context.Orders.Add(model);
			await _context.SaveChangesAsync();
			_context.CartItems.RemoveRange(model.CartItems);
			await _context.SaveChangesAsync();

			await _emailService.SendOrderConfirmEmail(user.Email, model);
			string adminEmail = _adminSettings.Email;
			await _emailService.SendOrderConfirmEmail(adminEmail, model);
			/////
			return RedirectToAction("OrderSummary", new { orderId = model.Id });
		}
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized("User not found.");
			}

			var cartItems = await _context.CartItems
									.Where(ci => ci.UserId == user.Id)
									.ToListAsync();

			if (!cartItems.Any())
			{
				return BadRequest("Your cart is empty.");
			}

			var orderModel = new Order();
			return View("~/Views/Marketplace/CheckoutView.cshtml", orderModel); 
		}		
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> OrderSummary(int orderId)
		{
			var order = await _context.Orders
							   .Include(c => c.CartItems)
							   .FirstOrDefaultAsync(c => c.Id == orderId);

			if (order == null || order.UserId != _userManager.GetUserId(User))
			{
				return NotFound();
			}

			return View("~/Views/Marketplace/OrderPlaced.cshtml", order);
		}
	}
}
