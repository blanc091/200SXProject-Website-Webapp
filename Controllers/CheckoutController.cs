using _200SXContact.Data;
using _200SXContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;

		public CheckoutController(ApplicationDbContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Checkout(Checkout model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized("User not found.");
			}

			model.UserId = user.Id;
			model.CartItems = await _context.CartItems
			.Where(ci => ci.UserId == user.Id)
									 .ToListAsync(); 

			_context.Checkouts.Add(model);
			await _context.SaveChangesAsync();

			_context.CartItems.RemoveRange(model.CartItems);
			await _context.SaveChangesAsync();

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

			var checkoutModel = new Checkout(); 
			return View(checkoutModel); 
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> PlaceOrder(Checkout model)
		{
			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized("User not found.");
			}

			model.UserId = user.Id;
			model.CartItems = await _context.CartItems
									   .Where(ci => ci.UserId == user.Id)
									   .ToListAsync();

			_context.Checkouts.Add(model);
			await _context.SaveChangesAsync();

			_context.CartItems.RemoveRange(model.CartItems);
			await _context.SaveChangesAsync();

			return RedirectToAction("OrderSummary", new { orderId = model.Id });
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> OrderSummary(int orderId)
		{
			var order = await _context.Checkouts
							   .Include(c => c.CartItems)
							   .FirstOrDefaultAsync(c => c.Id == orderId);

			if (order == null || order.UserId != _userManager.GetUserId(User))
			{
				return NotFound();
			}

			return View(order);
		}
	}
}
