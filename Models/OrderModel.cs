using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class Order
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		[Column(TypeName = "decimal(4,2)")]
		public decimal TotalAmount { get; set; }
		public DateTime OrderDate { get; set; } = DateTime.UtcNow;
				
		public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
	}
}
