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
		private readonly ILoggerService _loggerService;
		public CheckoutController(ApplicationDbContext context, UserManager<User> userManager, Services.IEmailService emailService, IOptions<AdminSettings> adminSettings, ILoggerService loggerService)
		{
			_emailService = emailService;
			_context = context;
			_userManager = userManager;
			_adminSettings = adminSettings.Value;
			_loggerService = loggerService;	
		}
		[HttpGet]
		[Route("checkout/view-checkout")]
		public async Task<IActionResult> Checkout()
		{
			await _loggerService.LogAsync("Checkout || Getting checkout view", "Info","");
			return View("~/Views/Marketplace/CheckoutView.cshtml");
		}
		[HttpPost]
		[Route("checkout/place-order")]
		[Authorize]		
		public async Task<IActionResult> PlaceOrder(Order model)
		{
            await _loggerService.LogAsync("Orders || Placing order", "Info", "");
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
                await _loggerService.LogAsync("Orders || No user found when placing order", "Error", "");
                TempData["IsUserLoggedIn"] = "no";
				TempData["Message"] = "You need to be registered and logged in to checkout.";
				return Redirect("/login-page");
			}
			model.UserId = user.Id;
			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
                    await _loggerService.LogAsync("Orders || Creating a new ordertracking instance in checkout view", "Info", "");
                    await _context.Orders.AddAsync(model);
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
					await _context.OrderTrackings.AddAsync(orderTracking);
					await _context.SaveChangesAsync();
                    await _loggerService.LogAsync("Orders || Created new ordertracking instance and saved to DB in checkout view", "Info", "");
                    var cartItems = await _context.CartItems
						.Where(ci => ci.UserId == user.Id)
						.ToListAsync();
					if (!cartItems.Any())
					{
                        await _loggerService.LogAsync("Orders || Cart is empty when trying to serialize in checkout view", "Error", "");
                        TempData["Message"] = "Your cart is empty, please add items before checking out.";
						return View("~/Views/Marketplace/CheckoutView.cshtml", model);
					}
					else
					{
						var cartItemsJson = JsonSerializer.Serialize(cartItems);
						model.CartItemsJson = cartItemsJson;
                        await _loggerService.LogAsync("Orders || Cart items serialized in checkout view", "Info", "");
                    }
					foreach (var cartItem in cartItems)
					{
						cartItem.OrderId = model.Id;
						_context.CartItems.Update(cartItem);
						_context.Orders.Update(model);
					}					
					await _context.SaveChangesAsync();                   
                    await transaction.CommitAsync();
                    await _loggerService.LogAsync("Orders || Added all cart items to DB and in context in checkout view", "Info", "");
                    await _loggerService.LogAsync("Orders || Creating orderwithItems in checkout view", "Info", "");
                    var orderWithItems = await _context.Orders
						.Include(o => o.CartItems)
						.FirstOrDefaultAsync(o => o.Id == model.Id);
                    await _loggerService.LogAsync("Orders || Sending confirmation emals to admin and user in checkout view", "Info", "");
                    await _emailService.SendOrderConfirmEmail(model.Email, orderWithItems);
					await _emailService.SendOrderConfirmEmail(_adminSettings.Email, orderWithItems);
					_context.CartItems.RemoveRange(cartItems);
					await _context.SaveChangesAsync();
                    await _loggerService.LogAsync("Orders || Removed cart items from cart and finished order process in checkout view", "Info", "");
                    return RedirectToAction("OrderSummary", new { orderId = model.Id });
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					TempData["Message"] = "Error: " + ex.Message;
                    await _loggerService.LogAsync("Orders || Error in checkout view " + ex.Message, "Error", "");
                    return RedirectToAction("Checkout");
				}
			}
		}		
		[HttpGet]
		[Route("checkout/your-order")]
		[Authorize]		
		public async Task<IActionResult> OrderSummary(int orderId)
		{
            await _loggerService.LogAsync("Orders || Getting order summary after order complete", "Info", "");
            var order = await _context.Orders
									   .FirstOrDefaultAsync(c => c.Id == orderId);
			if (order == null || order.UserId != _userManager.GetUserId(User))
			{
                await _loggerService.LogAsync("Orders || Order or user are null in order summary after order complete", "Error", "");
                return NotFound();
			}
			if (!string.IsNullOrEmpty(order.CartItemsJson))
			{
                await _loggerService.LogAsync("Orders || Getting cart items for order summary after order complete", "Info", "");
                var cartItems = JsonSerializer.Deserialize<List<CartItem>>(order.CartItemsJson);
				order.CartItems = cartItems;
                await _loggerService.LogAsync("Got cart items in order summary after order complete", "Info", "");
            }
            await _loggerService.LogAsync("Orders || Got order summary after order complete", "Info", "");
            return View("~/Views/Marketplace/OrderPlaced.cshtml", order);
		}		
	}
}
