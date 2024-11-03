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

	[HttpPost]
	public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
	{
		var product = await _context.Products.FindAsync(productId);
		if (product == null)
			return NotFound();

		var user = await _userManager.GetUserAsync(User); // Get the current authenticated user
		if (user == null)
			return Unauthorized();

		// Check if the item is already in the cart
		var existingCartItem = await _context.CartItems
			.FirstOrDefaultAsync(ci => ci.ProductId == productId && ci.UserId == user.Id);

		if (existingCartItem != null)
		{
			// Update quantity if already in cart
			existingCartItem.Quantity += quantity;
		}
		else
		{
			// Create a new CartItem
			var cartItem = new CartItem
			{
				ProductId = product.Id,
				ProductName = product.Name,
				Price = product.Price,
				Quantity = quantity,
				ImagePath = product.ImagePath,
				UserId = user.Id
			};

			await _context.CartItems.AddAsync(cartItem);
		}

		await _context.SaveChangesAsync();
		return RedirectToAction("Index");
	}
	
}