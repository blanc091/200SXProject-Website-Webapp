namespace _200SXContact.Models
{
	public class OrderTrackingViewModel
	{
		public Order Order { get; set; }
		public OrderTracking OrderTracking { get; set; }
		public List<CartItem> CartItems { get; set; } = new List<CartItem>();
	}
}
