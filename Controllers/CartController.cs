using Microsoft.AspNetCore.Mvc;
using _200SXContact.Models;
using _200SXContact.Data;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Services;

public class CartController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<User> _userManager;
	private readonly IEmailService _emailService; 

	public CartController(ApplicationDbContext context, UserManager<User> userManager, IEmailService emailService)
	{
		_context = context;
		_userManager = userManager;
		_emailService = emailService;
	}
	[HttpGet]
	public async Task<IActionResult> CartView()
	{
		var cartItems = await _context.CartItems.ToListAsync();
		return View("~/Views/Marketplace/CartView.cshtml", cartItems);
	}
	[HttpPost]
	public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
	{
		var product = await _context.Products.FindAsync(productId);
		if (product == null)
			return RedirectToAction("DetailedProductView", "Products", new { id = productId });

		var user = await _userManager.GetUserAsync(User);
		if (user == null) 
		{ 
			TempData["IsUserLoggedIn"] = "no";
			TempData["Message"] = "You need to be registered and logged in to add products to your cart.";
			return RedirectToAction("Login","LoginRegister");
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
		return RedirectToAction("ProductsDashboard", "Products");
	}
	[HttpGet]
	public async Task<IActionResult> GetCartItemCount()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return Json(0);
		}

		var cartItemCount = await _context.CartItems
			.Where(ci => ci.UserId == user.Id)
			.SumAsync(ci => ci.Quantity);

		return Json(cartItemCount);
	}
	[HttpPost]
	public async Task<IActionResult> RemoveFromCart(int productId)
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return Json(0);
		}

		var cartItem = await _context.CartItems
			.FirstOrDefaultAsync(ci => ci.ProductId == productId && ci.UserId == user.Id);

		if (cartItem == null)
			return NotFound();

		_context.CartItems.Remove(cartItem);
		await _context.SaveChangesAsync();
		var cartItems = await _context.CartItems.ToListAsync();
		return View("~/Views/Marketplace/CartView.cshtml", cartItems);
	}

}