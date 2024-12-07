using Microsoft.AspNetCore.Mvc;
using _200SXContact.Models;
using _200SXContact.Data;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Services;

namespace _200SXContact.Controllers
{
	public class CartController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly IEmailService _emailService;
		private readonly ILoggerService _loggerService;
		public CartController(ApplicationDbContext context, UserManager<User> userManager, IEmailService emailService, ILoggerService loggerService)
		{
			_context = context;
			_userManager = userManager;
			_emailService = emailService;
			_loggerService = loggerService;
		}
		[HttpGet]
		[Route("cart/view-cart")]
		public async Task<IActionResult> CartView()
		{
            await _loggerService.LogAsync("Getting cart view", "Info", "");
            var cartItems = await _context.CartItems.ToListAsync();
            await _loggerService.LogAsync("Got cart view", "Info", "");
            return View("~/Views/Marketplace/CartView.cshtml", cartItems);
		}
		[HttpPost]
		[Route("cart/add-item")]
		public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
		{
            await _loggerService.LogAsync("Adding item to cart", "Info", "");
            var product = await _context.Products.FindAsync(productId);
			if (product == null)
			{
                await _loggerService.LogAsync("Product is null when adding to cart", "Error", "");
                return RedirectToAction("DetailedProductView", "Products", new { id = productId });
            }				
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
                await _loggerService.LogAsync("User is null or not logged in when adding to cart", "Error", "");
                TempData["IsUserLoggedIn"] = "no";
				TempData["Message"] = "You need to be registered and logged in to add products to your cart.";
				return Redirect("/login-page");
			}
			var existingCartItem = await _context.CartItems
				.FirstOrDefaultAsync(ci => ci.ProductId == productId && ci.UserId == user.Id);
			if (existingCartItem != null)
			{
				existingCartItem.Quantity += quantity;
			}
			else
			{
				var primaryImagePath = product.ImagePaths?.FirstOrDefault() ?? "/images/default-placeholder.png";
				var cartItem = new CartItem
				{
					ProductId = product.Id,
					ProductName = product.Name,
					Price = product.Price,
					Quantity = quantity,
					ImagePath = primaryImagePath,
					UserId = user.Id
				};
				await _context.CartItems.AddAsync(cartItem);
			}
			await _context.SaveChangesAsync();
			TempData["ItemAdded"] = "yes";
			TempData["Message"] = "Item added to cart !";
            await _loggerService.LogAsync("Added item to cart", "Info", "");
            return RedirectToAction("ProductsDashboard", "Products");
		}
		[HttpGet]
		[Route("cart/get-cart-items")]
		public async Task<IActionResult> GetCartItemCount()
		{
            await _loggerService.LogAsync("Getting cart items count", "Info", "");
            var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
                await _loggerService.LogAsync("User is null when getting cart items", "Errors", "");
                return Json(0);
			}
			var cartItemCount = await _context.CartItems
				.Where(ci => ci.UserId == user.Id)
				.SumAsync(ci => ci.Quantity);
            await _loggerService.LogAsync("Got cart items", "Info", "");
            return Json(cartItemCount);
		}
		[HttpPost]
		[Route("cart/remove-cart-item")]
		public async Task<IActionResult> RemoveFromCart(int productId)
		{
            await _loggerService.LogAsync("Removing cart item", "Info", "");
            var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
                await _loggerService.LogAsync("User is null when removing cart items", "Error", "");
                return Json(0);
			}
			var cartItem = await _context.CartItems
				.FirstOrDefaultAsync(ci => ci.ProductId == productId && ci.UserId == user.Id);
			if (cartItem == null)
			{
				await _loggerService.LogAsync("Cart item is null when removing cart items", "Error", "");
				return NotFound();
			}
			_context.CartItems.Remove(cartItem);
			await _context.SaveChangesAsync();
			var cartItems = await _context.CartItems.ToListAsync();
            await _loggerService.LogAsync("Removed cart item", "Info", "");
            return View("~/Views/Marketplace/CartView.cshtml", cartItems);
		}
	}
}