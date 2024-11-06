using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class OrderTracking
	{
		public int Id { get; set; } 

		[Required]
		[ForeignKey("Order")]
		public int OrderId { get; set; } 

		[Required]
		[StringLength(50)]
		public string Status { get; set; }  

		[StringLength(100)]
		public string Carrier { get; set; } 

		[StringLength(50)]
		public string TrackingNumber { get; set; } 

		public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;  

		public virtual Order Order { get; set; } 
	}
}
