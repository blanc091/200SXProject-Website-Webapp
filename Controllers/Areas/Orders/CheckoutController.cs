using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;
using _200SXContact.Commands.Areas.Orders;
using _200SXContact.Queries.Areas.Orders;
using FluentValidation;
using FluentValidation.Results;
using System.Diagnostics;
using _200SXContact.Interfaces.Areas.Admin;

namespace _200SXContact.Controllers.Areas.Orders
{
    public class CheckoutController : Controller
	{
		private readonly ILoggerService _loggerService;
        private readonly IMediator _mediator;
        private readonly IValidator<PlaceOrderCommand> _validator;
        public CheckoutController(ILoggerService loggerService, IMediator mediator, IValidator<PlaceOrderCommand> validator)
		{
			_loggerService = loggerService;
            _mediator = mediator;
            _validator = validator;
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
        public async Task<IActionResult> PlaceOrder(OrderPlacementDto model)
        {
            await _loggerService.LogAsync("Orders || Placing order", "Info", "");

            ModelState.Remove("UserId");
            ModelState.Remove("CartItems");
            ModelState.Remove("OrderItems");
            ModelState.Remove("User");
            ModelState.Remove("CartItemsJson");

            await _loggerService.LogAsync("Orders || Removed initial model states", "Info", "");

            ValidationResult validationResult = await _validator.ValidateAsync(new PlaceOrderCommand(model, User));

            if (!validationResult.IsValid)
            {
                await _loggerService.LogAsync("Orders || Model state is invalid", "Error", "");

                foreach (ValidationFailure? error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    Debug.WriteLine(error.ToString());
                }

                return View("~/Views/Marketplace/CheckoutView.cshtml", model);
            }

            if (!ModelState.IsValid)
            {
                await _loggerService.LogAsync("Orders || Model state is invalid", "Error", "");

                return View("~/Views/Marketplace/CheckoutView.cshtml", model);
            }

            try
            {
                int orderId = await _mediator.Send(new PlaceOrderCommand(model, User));
               
                return RedirectToAction("OrderSummary", new { orderId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["IsUserLoggedIn"] = "no";
                TempData["Message"] = "You need to be registered and logged in to checkout";

                return Redirect("/login-page");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Message"] = ex.Message;

                return View("~/Views/Marketplace/CheckoutView.cshtml", model);
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync("Orders || Error in checkout: " + ex.Message, "Error", "");

                TempData["Message"] = "An error occurred while placing your order. Please try again.";

                return RedirectToAction("Checkout");
            }
        }
        [HttpGet]
        [Route("checkout/your-order")]
        [Authorize]
        public async Task<IActionResult> OrderSummary(int orderId)
        {
            var orderUserDashDto = await _mediator.Send(new GetOrderSummaryQuery(orderId));

            if (orderUserDashDto == null)
            {
                await _loggerService.LogAsync("Checkout || Error when getting model for order summary", "Error", "");

                return NotFound();
            }

            return View("~/Views/Marketplace/OrderPlaced.cshtml", orderUserDashDto);
        }
    }
}
