using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Models.DTOs;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace _200SXContact.Controllers
{
	public class PendingOrdersController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly ILoggerService _loggerService;
		public PendingOrdersController(ApplicationDbContext context, UserManager<User> userManager, ILoggerService loggerService)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
		}
		[HttpGet]
		[Route("pendingorders/view-my-orders")]
		[Authorize]
		public async Task<IActionResult> UserOrders()
		{
            await _loggerService.LogAsync("Getting user orders page", "Info", "");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var orders = await _context.Orders
				.Where(o => o.UserId == userId)
				.ToListAsync();
			var orderViewModels = orders.Select(order => new OrderTrackingViewModel
			{
				Order = order,
				OrderTracking = _context.OrderTrackings.FirstOrDefault(ot => ot.OrderId == order.Id),
				CartItems = string.IsNullOrWhiteSpace(order.CartItemsJson)
							? new List<CartItem>()
							: JsonSerializer.Deserialize<List<CartItem>>(order.CartItemsJson)
			}).ToList();
            await _loggerService.LogAsync("Got user orders page", "Info", "");
            return View("~/Views/Marketplace/PendingOrdersCustomer.cshtml", orderViewModels);
		}
		[HttpGet]
		[Route("pendingorders/get-all-orders-admin")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAllOrders()
		{
            await _loggerService.LogAsync("Getting all orders admin", "Info", "");
            var orders = await _context.Orders
				.Include(o => o.CartItems) 
				.ToListAsync();
			var orderTrackings = await _context.OrderTrackings.ToListAsync();
			var viewModels = orders.Select(order => new OrderTrackingViewModel
			{
				Order = order,
				OrderTracking = orderTrackings.FirstOrDefault(ot => ot.OrderId == order.Id),
				CartItems = order.CartItems.ToList()
			}).ToList();
            await _loggerService.LogAsync("Got all orders admin", "Info", "");
            return View("~/Views/Marketplace/UpdateCustomerOrder.cshtml", viewModels);
		}
		[HttpGet]
		[Route("pendingorders/get-cart-items")]
		[Authorize(Roles = "Admin")]		
		public async Task<IActionResult> GetCartItems(int orderId)
		{
            await _loggerService.LogAsync("Getting cart items admin for orders", "Info", "");
            var order = await _context.Orders
				.Where(o => o.Id == orderId)
				.Select(o => o.CartItemsJson)
				.FirstOrDefaultAsync();
			if (order == null)
			{
                await _loggerService.LogAsync("No order found when trying to get cart items for order", "Error", "");
                return NotFound("Order not found.");
			}
			var cartItems = JsonSerializer.Deserialize<List<CartItem>>(order);
			if (cartItems == null || !cartItems.Any())
			{
                await _loggerService.LogAsync("No cart items found for the specified order " + order, "Error", "");
                return NotFound("No cart items found for the specified order.");
			}
			var cartItemsViewModel = cartItems.Select(ci => new
			{
				ProductName = ci.ProductName,
				Quantity = ci.Quantity,
				Price = ci.Price
			});
            await _loggerService.LogAsync("Got cart items admin for orders", "Info", "");
            return Json(cartItemsViewModel);
		}
		[HttpPost]
		[Route("pendingorders/update-order-tracking")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateOrderTrackingAjax([FromBody] OrderTrackingUpdateDto updateDto)
		{
            await _loggerService.LogAsync("Started updating order tracking details admin", "Info", "");
            if (updateDto == null || updateDto.OrderId == 0)
			{
                await _loggerService.LogAsync("Invalid data received in admin interface for order tracking update AJAX", "Error", "");
                return BadRequest("Invalid data received.");
			}
			var orderTracking = await _context.OrderTrackings
				.FirstOrDefaultAsync(ot => ot.OrderId == updateDto.OrderId);
			if (orderTracking == null)
			{
                await _loggerService.LogAsync("Order tracking not found in admin interface for " + updateDto.OrderId, "Error", "");
                return NotFound("Order tracking record not found.");
			}
			orderTracking.Status = updateDto.Status;
			orderTracking.Carrier = updateDto.Carrier;
			orderTracking.TrackingNumber = updateDto.TrackingNumber;
			orderTracking.StatusUpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();
            await _loggerService.LogAsync("Updated order tracking details admin for " + updateDto.OrderId, "Info", "");
            return Ok(new { message = "Order tracking updated successfully!" });
		}
	}
}

