using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class OrderTracking
	{
		public int Id { get; set; } 
		[ForeignKey("Order")]
		public int OrderId { get; set; }
		public string Status { get; set; }  
		public string? Carrier { get; set; }
		public string? TrackingNumber { get; set; } 
		public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;
		public string Email { get; set; }
		public string AddressLine { get; set; }
		public string OrderNotes { get; set; }						
		public string? CartItemsJson { get; set; }
		public virtual Order Order { get; set; } 
	}
}
