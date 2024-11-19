using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace _200SXContact.Controllers
{
	public class PendingOrdersController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;

		public PendingOrdersController(ApplicationDbContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> OrderSummary(int orderId)
		{
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.Id == orderId);

			if (order == null || order.UserId != _userManager.GetUserId(User))
			{
				return NotFound();
			}

			var orderTracking = await _context.OrderTrackings
				.FirstOrDefaultAsync(ot => ot.OrderId == orderId);

			if (orderTracking == null)
			{
				return NotFound();
			}

			var cartItemsJson = order.CartItemsJson ?? orderTracking.CartItemsJson;
			var cartItems = string.IsNullOrEmpty(cartItemsJson)
				? new List<CartItem>()
				: JsonSerializer.Deserialize<List<CartItem>>(cartItemsJson);

			var viewModel = new OrderTrackingViewModel
			{
				Order = order,
				OrderTracking = orderTracking,
				CartItems = cartItems
			};

			return View("~/Views/Marketplace/PendingOrdersCustomer.cshtml", viewModel);
		}
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAllOrders()
		{
			// Fetch all orders along with their related data
			var orders = await _context.Orders
				.Include(o => o.CartItems) // Include cart items
				.ToListAsync();

			var orderTrackings = await _context.OrderTrackings.ToListAsync();

			// Map the data into a list of view models
			var viewModels = orders.Select(order => new OrderTrackingViewModel
			{
				Order = order,
				OrderTracking = orderTrackings.FirstOrDefault(ot => ot.OrderId == order.Id),
				CartItems = order.CartItems.ToList()
			}).ToList();

			// Pass the list of view models to the view
			return View("~/Views/Marketplace/UpdateCustomerOrder.cshtml", viewModels);
		}
		[HttpGet]
		[Authorize(Roles = "Admin")]
		[Route("PendingOrders/GetCartItems")]
		public async Task<IActionResult> GetCartItems(int orderId)
		{
			var order = await _context.Orders
				.Where(o => o.Id == orderId)
				.Select(o => o.CartItemsJson)
				.FirstOrDefaultAsync();

			if (order == null)
			{
				return NotFound("Order not found.");
			}

			var cartItems = JsonSerializer.Deserialize<List<CartItem>>(order);

			if (cartItems == null || !cartItems.Any())
			{
				return NotFound("No cart items found for the specified order.");
			}

			var cartItemsViewModel = cartItems.Select(ci => new
			{
				ProductName = ci.ProductName,
				Quantity = ci.Quantity,
				Price = ci.Price
			});

			return Json(cartItemsViewModel);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateOrderTrackingAjax([FromBody] OrderTrackingUpdateDto updateDto)
		{
			if (updateDto == null || updateDto.OrderId == 0)
			{
				return BadRequest("Invalid data received.");
			}

			// Fetch the OrderTracking record
			var orderTracking = await _context.OrderTrackings
				.FirstOrDefaultAsync(ot => ot.OrderId == updateDto.OrderId);

			if (orderTracking == null)
			{
				return NotFound("Order tracking record not found.");
			}

			// Update fields
			orderTracking.Status = updateDto.Status;
			orderTracking.Carrier = updateDto.Carrier;
			orderTracking.TrackingNumber = updateDto.TrackingNumber;
			orderTracking.StatusUpdatedAt = DateTime.UtcNow;

			// Save changes
			await _context.SaveChangesAsync();

			return Ok(new { message = "Order tracking updated successfully!" });
		}


	}

}

