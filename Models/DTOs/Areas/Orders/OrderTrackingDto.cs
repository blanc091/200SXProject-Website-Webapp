namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderTrackingDto
    {
        public int OrderId { get; set; }
        public required string Status { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
        public string? AddressLine { get; set; }
        public required string Email { get; set; }
        public string? OrderNotes { get; set; }
        public required string CartItemsJson { get; set; }
    }
}
