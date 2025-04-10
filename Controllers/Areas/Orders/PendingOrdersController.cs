using _200SXContact.Commands.Areas.Orders;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Orders;
using _200SXContact.Queries.Areas.Orders;

namespace _200SXContact.Controllers.Areas.Orders
{
    public class PendingOrdersController : Controller
	{
		private readonly ILoggerService _loggerService;
		private readonly IMediator _mediator;
        public PendingOrdersController(ILoggerService loggerService, IMediator mediator)
		{
			_loggerService = loggerService;
			_mediator = mediator;
        }
		[HttpGet]
		[Route("pendingorders/view-my-orders")]
		[Authorize]
		public async Task<IActionResult> UserOrders(string? userId)
		{
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    await _loggerService.LogAsync("Orders || User ID not found", "Warning", "");

                    TempData["Message"] = "Please log in to view your orders.";

                    return Redirect("/login-page");
                }
            }

            List<OrderUserDashDto>? orders = await _mediator.Send(new GetUserOrdersQuery(userId));

            if (orders == null || orders.Count == 0)
            {
                await _loggerService.LogAsync("Orders || No orders found for user id " + userId, "Warning", "");

                TempData["Message"] = "No orders found for user.";
            }

            return View("~/Views/Marketplace/PendingOrdersCustomer.cshtml", orders);
        }
        [HttpGet]
        [Route("pendingorders/get-user-orders-json")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserOrdersJson(string? orderId)
        {
            int? parsedOrderId = null;

            if (!string.IsNullOrEmpty(orderId) && int.TryParse(orderId, out int id))
            {
                parsedOrderId = id;
            }

            List<OrderUserDashDto>? orders = await _mediator.Send(new GetUserOrdersQuery("", parsedOrderId));

            if (orders == null || orders.Count == 0)
            {
                await _loggerService.LogAsync("Orders || No order details found for order id " + orderId, "Warning", "");

                return NotFound("No orders found for user.");
            }

            return Ok(orders);
        }
        [HttpGet]
        [Route("pendingorders/get-all-orders-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            List<OrderTrackingUpdateDto> orders = await _mediator.Send(new GetAllOrdersQuery());

            if (orders == null || orders.Count == 0)
            {
                await _loggerService.LogAsync("Orders || No orders found at all", "Error", "");
                // Optionally, you could also return NotFound or a custom message.
                return Ok(new List<OrderTrackingUpdateDto>());
            }

            return Ok(orders);
        }
        [HttpGet]
        [Route("pendingorders/get-cart-items")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCartItems(int orderId)
        {
            try
            {
                List<Models.Areas.Orders.CartItem>? cartItems = await _mediator.Send(new GetCartItemsQuery(orderId));

                if (cartItems != null && cartItems.Count == 0)
                {
                    await _loggerService.LogAsync($"Pending Orders Admin || No cart items found for order {orderId}", "Error", "");

                    return NotFound("No cart items found for the specified order.");                    
                }

                var cartItemsViewModel = cartItems.Select(ci => new
                {
                    ci.ProductName,
                    ci.Quantity,
                    ci.Price
                });

                return Json(cartItemsViewModel);
            }
            catch (KeyNotFoundException)
            {
                TempData["Message"] = "No cart items found for order " + orderId.ToString();

                return NotFound("No cart items found.");
            }
        }
        [HttpPost]
        [Route("pendingorders/update-order-tracking")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderTrackingAjax([FromBody] OrderTrackingUpdateDto updateDto)
        {
            if (updateDto == null || updateDto.OrderId == 0)
            {
                await _loggerService.LogAsync("Orders || Invalid data received in admin interface for order tracking update AJAX", "Error", "");

                return BadRequest(new { success = false, message = "Invalid data received !" });
            }

            UpdateOrderTrackingCommandHandler.UpdateOrderTrackingCommandResult result = await _mediator.Send(new UpdateOrderTrackingCommand(updateDto));

            if (!result.Succeeded)
            {
                string errorMessages = string.Join("; ", result.Errors.SelectMany(e => e.Value));

                await _loggerService.LogAsync($"Orders || Error updating order tracking for {updateDto.OrderId}: {errorMessages}", "Error", "");

                return BadRequest(errorMessages);
            }

            return Ok(new { success = true, message = "Order tracking updated successfully !" });
        }
    }
}

