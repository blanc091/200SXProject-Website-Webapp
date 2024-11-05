using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class OrderItem
	{
		public int Id { get; set; }
		[ForeignKey("Order")]
		public int OrderId { get; set; }
		public Order Order { get; set; }
		[ForeignKey("CartItem")]
		public int CartItemId { get; set; }
		public CartItem CartItem { get; set; }
		public int Quantity { get; set; }
		[Column(TypeName = "decimal(4,2)")]
		public decimal Price { get; set; } 
		public string ProductName { get; set; }
	}
}
