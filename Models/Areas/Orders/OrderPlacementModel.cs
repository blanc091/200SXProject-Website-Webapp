using _200SXContact.Interfaces.Areas.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Orders
{
    public class OrderPlacement : IOrderPlacement
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }
        [Required]
        [Display(Name = "Address Line 1")]
        public string? AddressLine1 { get; set; }
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        [Display(Name = "County or State")]
        public string? County { get; set; }
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public required string Email { get; set; }
        [Display(Name = "Order Notes")]
        public string? OrderNotes { get; set; }
        public DateTime OrderDate { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }
        public required string CartItemsJson { get; set; }
        public required List<CartItemModel> CartItems { get; set; } = new();
        public virtual OrderTracking? OrderTracking { get; set; }
    }
}
