using System.Text;
using Newtonsoft.Json;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace _200SXContact.Services
{
	public class PayPalHelperService
	{
		private readonly string _paypalClientId;
		private readonly string _paypalClientSecret;
		private readonly string _paypalApiUrl = "https://api-m.sandbox.paypal.com"; // PayPal sandbox URL
		private readonly ILogger<PayPalHelperService> _logger;

		public PayPalHelperService(IConfiguration configuration, ILogger<PayPalHelperService> logger)
		{
			_logger = logger;

			// Get PayPal credentials from appsettings.json
			_paypalClientId = configuration["PayPal:ClientId"];
			_paypalClientSecret = configuration["PayPal:ClientSecret"];
		}

		private async Task<string> GetAccessTokenAsync()
		{
			var client = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Post, $"{_paypalApiUrl}/v1/oauth2/token")
			{
				Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "grant_type", "client_credentials" }
				})
			};

			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
				"Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_paypalClientId}:{_paypalClientSecret}"))
			);

			var response = await client.SendAsync(request);
			var responseString = await response.Content.ReadAsStringAsync();
			dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

			return jsonResponse.access_token;
		}

		public async Task<string> CreateOrderAsync(decimal amount, string currency)
		{
			string accessToken = await GetAccessTokenAsync();
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
			client.DefaultRequestHeaders.Add("Content-Type", "application/json");

			var orderRequest = new
			{
				intent = "CAPTURE",
				purchase_units = new[]
				{
					new
					{
						amount = new { currency_code = currency, value = amount.ToString("0.00") }
					}
				},
				application_context = new
				{
					return_url = "http://yourwebsite.com/return",
					cancel_url = "http://yourwebsite.com/cancel"
				}
			};

			var content = new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"{_paypalApiUrl}/v2/checkout/orders", content);
			var responseString = await response.Content.ReadAsStringAsync();

			dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
			return jsonResponse.id;
		}

		public async Task<bool> CapturePaymentAsync(string orderId)
		{
			string accessToken = await GetAccessTokenAsync();
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

			var response = await client.PostAsync($"{_paypalApiUrl}/v2/checkout/orders/{orderId}/capture", null);
			return response.IsSuccessStatusCode;
		}
	}
}