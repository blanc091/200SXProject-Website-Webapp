using _200SXContact.Interfaces.Areas.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Orders
{
    public class OrderTracking : IOrderTracking
    {
        [Key]
        public required int Id { get; set; }
        public required int OrderId { get; set; }
        public required string Status { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public required DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;
        public required string Email { get; set; }
        public string? AddressLine { get; set; }
        public string? OrderNotes { get; set; }
        public string? CartItemsJson { get; set; }
        public virtual required OrderPlacement Order { get; set; }
    }
}
