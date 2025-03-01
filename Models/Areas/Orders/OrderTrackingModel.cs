using _200SXContact.Interfaces.Areas.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Orders
{
    public class OrderTracking : IOrderTracking
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        [Required, StringLength(50)]
        public required string Status { get; set; }
        [StringLength(50)]
        public string? Carrier { get; set; }
        [StringLength(100)]
        public string? TrackingNumber { get; set; }
        public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public required string Email { get; set; }
        [Required]
        public string? AddressLine { get; set; }
        public string? OrderNotes { get; set; }
        public string? CartItemsJson { get; set; }
        public virtual required OrderPlacement Order { get; set; }
    }
}
