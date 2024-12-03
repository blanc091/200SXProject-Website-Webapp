using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using _200SXContact.Services;
using _200SXContact.Models.Configs;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace _200SXContact.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly Services.IEmailService _emailService;
		private readonly AdminSettings _adminSettings;
		public CheckoutController(ApplicationDbContext context, UserManager<User> userManager, Services.IEmailService emailService, IOptions<AdminSettings> adminSettings)
		{
			_emailService = emailService;
			_context = context;
			_userManager = userManager;
			_adminSettings = adminSettings.Value;
		}
		[HttpGet]
		[Route("checkout/view-checkout")]
		public IActionResult Checkout()
		{
			return View("~/Views/Marketplace/CheckoutView.cshtml");
		}
		[HttpPost]
		[Route("checkout/place-order")]
		[Authorize]		
		public async Task<IActionResult> PlaceOrder(Order model)
		{
			ModelState.Remove("UserId");
			ModelState.Remove("CartItems");
			ModelState.Remove("OrderItems");
			ModelState.Remove("User");
			ModelState.Remove("CartItemsJson");
			if (!ModelState.IsValid)
			{
				return View("~/Views/Marketplace/CheckoutView.cshtml", model);
			}
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				TempData["IsUserLoggedIn"] = "no";
				TempData["Message"] = "You need to be registered and logged in to checkout.";
				return Redirect("/login-page");
			}
			model.UserId = user.Id;
			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{ 
					_context.Orders.Add(model);
					await _context.SaveChangesAsync();
					var orderTracking = new OrderTracking
					{
						OrderId = model.Id,
						Status = "Pending",
						StatusUpdatedAt = DateTime.UtcNow,
						Email = model.Email,
						AddressLine = model.AddressLine1,
						OrderNotes = model.OrderNotes,
						CartItemsJson = model.CartItemsJson
					};
					_context.OrderTrackings.Add(orderTracking);
					await _context.SaveChangesAsync();
					var cartItems = await _context.CartItems
						.Where(ci => ci.UserId == user.Id)
						.ToListAsync();

					if (!cartItems.Any())
					{
						TempData["Message"] = "Your cart is empty, please add items before checking out.";
						return View("~/Views/Marketplace/CheckoutView.cshtml", model);
					}
					else
					{
						var cartItemsJson = JsonSerializer.Serialize(cartItems);
						model.CartItemsJson = cartItemsJson;
					}
					foreach (var cartItem in cartItems)
					{
						cartItem.OrderId = model.Id;
						_context.CartItems.Update(cartItem);
						_context.Orders.Update(model);
					}					
					await _context.SaveChangesAsync(); 
					await transaction.CommitAsync();
					var orderWithItems = await _context.Orders
						.Include(o => o.CartItems)
						.FirstOrDefaultAsync(o => o.Id == model.Id);

					await _emailService.SendOrderConfirmEmail(model.Email, orderWithItems);
					await _emailService.SendOrderConfirmEmail(_adminSettings.Email, orderWithItems);
					_context.CartItems.RemoveRange(cartItems);
					await _context.SaveChangesAsync();
					return RedirectToAction("OrderSummary", new { orderId = model.Id });
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					TempData["Message"] = "Error: " + ex.Message;
					return RedirectToAction("Checkout");
				}
			}
		}		
		[HttpGet]
		[Route("checkout/your-order")]
		[Authorize]		
		public async Task<IActionResult> OrderSummary(int orderId)
		{
			var order = await _context.Orders
									   .FirstOrDefaultAsync(c => c.Id == orderId);
			if (order == null || order.UserId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			if (!string.IsNullOrEmpty(order.CartItemsJson))
			{
				var cartItems = JsonSerializer.Deserialize<List<CartItem>>(order.CartItemsJson);

				order.CartItems = cartItems;
			}
			return View("~/Views/Marketplace/OrderPlaced.cshtml", order);
		}		
	}
}
