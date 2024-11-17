using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models
{
	public class CartItem
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }

		[Column(TypeName = "decimal(4,2)")]
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public string ImagePath { get; set; }
		public string UserId { get; set; }
		public int? OrderId { get; set; }
		public Order Order { get; set; }
	}
}
