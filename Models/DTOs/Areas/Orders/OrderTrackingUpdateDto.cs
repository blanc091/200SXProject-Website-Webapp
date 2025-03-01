using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderTrackingUpdateDto
    {
        [Required]
        public int OrderId { get; set; }
        [Required, StringLength(50)]
        public required string Status { get; set; }
        [StringLength(50)]
        public string? Carrier { get; set; }
        [StringLength(100)]
        public string? TrackingNumber { get; set; }
    }
}
