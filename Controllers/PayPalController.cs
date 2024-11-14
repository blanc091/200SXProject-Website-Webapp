using _200SXContact.Services;
using Microsoft.AspNetCore.Mvc;
using PayPalCheckoutSdk.Orders;
using System.Threading.Tasks;

namespace _200SXContact.Controllers
{
	public class PayPalController : Controller
	{
		private readonly PayPalHelperService _paypalHelperService;

		public PayPalController(PayPalHelperService paypalHelperService)
		{
			_paypalHelperService = paypalHelperService;
		}
		[HttpGet]
		public IActionResult PayPalProcessor()
		{
			return View("~/Views/Marketplace/PayPalProcessor.cshtml");
		}
		[HttpPost]
		public async Task<IActionResult> CreateOrder(decimal amount)
		{
			try
			{
				string orderId = await _paypalHelperService.CreateOrderAsync(amount, "USD");
				return Redirect($"https://www.sandbox.paypal.com/checkoutnow?token={orderId}");
			}
			catch (Exception ex)
			{
				// Log error
				return View("Error", new { message = "Error creating order" });
			}
		}

		[HttpGet]
		public async Task<IActionResult> CapturePayment(string orderId)
		{
			try
			{
				bool success = await _paypalHelperService.CapturePaymentAsync(orderId);
				if (success)
				{
					return View("PaymentSuccess");
				}
				return View("PaymentFailure");
			}
			catch (Exception ex)
			{
				// Log error
				return View("Error", new { message = "Error capturing payment" });
			}
		}
	}
}