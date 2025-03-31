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

            if (orders == null)
            {
                await _loggerService.LogAsync("Orders || No orders found for user id " + userId, "Warning", "");

                TempData["Message"] = "No orders found for user.";
            }

            return View("~/Views/Marketplace/PendingOrdersCustomer.cshtml", orders);
        }
		[HttpGet]
		[Route("pendingorders/get-all-orders-admin")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAllOrders()
		{
            List<OrderTrackingUpdateDto> orders = await _mediator.Send(new GetAllOrdersQuery());

            if (orders is null)
            {
                await _loggerService.LogAsync("Orders || No orders found at all", "Error", "");

                TempData["Message"] = "No orders found at all !";   
            }

            return View("~/Views/Marketplace/UpdateCustomerOrder.cshtml", orders);
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

                return BadRequest("Invalid data received.");
            }

            bool success = await _mediator.Send(new UpdateOrderTrackingCommand(updateDto));

            if (!success)
            {
                await _loggerService.LogAsync($"Orders || Order tracking not found for {updateDto.OrderId}", "Error", "");

                return NotFound("Order tracking record not found.");
            }

            return Ok(new { message = "Order tracking updated successfully !" });
        }
    }
}

