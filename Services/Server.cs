using System.Collections.Generic;
using System.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using Stripe.Checkout;

public class StripeOptions
{
	public string option { get; set; }
}

namespace _200SXContact.Services
{
	public class Program
	{

		public static void Main(string[] args)
		{
			WebHost.CreateDefaultBuilder(args)
			  .UseUrls("https://200sxproject.azurewebsites.net", "https://localhost:7109")
			  .UseWebRoot("public")
			  .UseStartup<Startup>()
			  .Build()
			  .Run();
		}
	}

	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}
		public IConfiguration Configuration { get; }
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.Configure<StripeOptions>(Configuration.GetSection("Stripe"));
		}
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{

			var stripeSecretKey = Configuration.GetValue<string>("Stripe:SecretKey");
			StripeConfiguration.ApiKey = stripeSecretKey;

			if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
			app.UseRouting();
			app.UseStaticFiles();
			app.UseEndpoints(endpoints => endpoints.MapControllers());
		}
	}

	[Route("create-checkout-session")]
	[ApiController]
	public class CheckoutApiController : Controller
	{
		[HttpPost]
		public ActionResult Create()
		{
			var domain = "https://200sxproject.azurewebsites.net";
			//var domain = "https://localhost:7109";
			var options = new SessionCreateOptions
			{
				UiMode = "embedded",
				LineItems = new List<SessionLineItemOptions>
				{
				  new SessionLineItemOptions
				  {
					Price = "price_1QL1xAKYqPJ7nhaliyDGEQPV",
					Quantity = 1,
				  },
				},
				Mode = "payment",
				ReturnUrl = domain + "/stripe/order-placed?session_id={CHECKOUT_SESSION_ID}",
				AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
			};
			var service = new SessionService();
			Session session = service.Create(options);

			return Json(new { clientSecret = session.ClientSecret });
		}
	}

	[Route("session-status")]
	[ApiController]
	public class SessionStatusController : Controller
	{
		[HttpGet]
		public ActionResult SessionStatus([FromQuery] string session_id)
		{
			var sessionService = new SessionService();
			Session session = sessionService.Get(session_id);

			return Json(new { status = session.Status, customer_email = session.CustomerDetails.Email });
		}
	}
}