using _200SXContact.Interfaces.Areas.Orders;
using _200SXContact.Models.Areas.UserContent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Orders
{
    public class OrderPlacement : IOrderPlacement
    {
        public required int Id { get; set; }
        public required string FullName { get; set; }
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public required string City { get; set; }
        public required string County { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public string? OrderNotes { get; set; }
        public DateTime OrderDate { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
        public required string CartItemsJson { get; set; }
        public required List<CartItem> CartItems { get; set; } = new();
        public virtual OrderTracking? OrderTracking { get; set; }
    }
}
