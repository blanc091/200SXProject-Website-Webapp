using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class Order
	{
		public int Id { get; set; }
		[Required]
		[Display(Name = "Full Name")]
		public string FullName { get; set; }
		[Required]
		[Display(Name = "Address Line 1")]
		public string AddressLine1 { get; set; }
		[Display(Name = "Address Line 2")]
		public string AddressLine2 { get; set; }
		[Required]
		public string City { get; set; }
		[Required]
		[Display(Name = "County or State")]
		public string County { get; set; }
		[Required]
		[Phone]
		[Display(Name = "Phone Number")]
		public string PhoneNumber { get; set; }
		[Required]
		[EmailAddress]
		[Display(Name = "Email Address")]
		public string Email { get; set; }
		[Display(Name = "Order Notes")]
		[Required]
		public string OrderNotes { get; set; }
		public DateTime OrderDate { get; set; } = DateTime.UtcNow;
		[ForeignKey("User")]
		public string UserId { get; set; }
		public User User { get; set; }
		public virtual ICollection<CartItem> CartItems { get; set; }
		public string? CartItemsJson { get; set; }
	}
}
