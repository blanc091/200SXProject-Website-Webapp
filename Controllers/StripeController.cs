using _200SXContact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace _200SXContact.Controllers
{
	public class StripeController : Controller
	{
        private readonly ILoggerService _loggerService;
        public StripeController(ILoggerService loggerService) 
		{
			_loggerService = loggerService;
		}       
        [HttpGet]
		[Route("stripe/payment")]
		public IActionResult StripeProcessor()
		{
            _loggerService.LogAsync("Stripe || Got Stripe payment page", "Info", "");
            return View("~/Views/Marketplace/StripeProcessor.cshtml");
		}
		[Route("Stripe")]
		[HttpPost("stripe/create-checkout-session")]
		public IActionResult CreateCheckoutSession()
		{
            _loggerService.LogAsync("Stripe || Starting to create Stripe checkout session", "Info", "");
            var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string> { "card" },
				LineItems = new List<SessionLineItemOptions>
				{
					new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = "Stubborn Attachments",
							},
							UnitAmount = 2000, // Price in cents ($20.00)
						},
						Quantity = 1,
					},
				},
					Mode = "payment",
					SuccessUrl = "https://yourdomain.com/success", // Replace with your success URL
					CancelUrl = "https://yourdomain.com/cancel",   // Replace with your cancel URL
			};
			var service = new SessionService();
			Session session = service.Create(options);
            _loggerService.LogAsync("Stripe || Created Stripe checkout session", "Info", "");
            return RedirectToAction("OrderPlaced", new { sessionId = session.Id });
		}
		[HttpPost]
		[Route("stripe/order-placed")]
		[Authorize]
		public IActionResult OrderPlaced(string sessionId)
		{
            _loggerService.LogAsync("Stripe || Starting Stripe OrderPlaced action", "Info", "");
            var service = new SessionService();
			var session = service.Get(sessionId);
			if (string.IsNullOrEmpty(sessionId))
			{
                _loggerService.LogAsync("Stripe || Stripe payment session id is missing", "Error", "");
                ViewBag.Message = "Session ID is missing!";
				return View("~/Views/Marketplace/OrderPlaced.cshtml");
			}
			if (session.PaymentStatus == "paid")
			{
                _loggerService.LogAsync("Stripe || Stripe payment successful", "Info", "");
                ViewBag.Message = "Payment successful!";
			}
			else
			{
                _loggerService.LogAsync("Stripe || Stripe payment failed", "Error", "");
                ViewBag.Message = "Payment failed.";
			}
			return View("~/Views/Marketplace/OrderPlaced.cshtml");
		}
	}
}
