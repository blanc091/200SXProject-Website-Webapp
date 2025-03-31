namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderTrackingUpdateDto
    {
        public required int OrderId { get; set; }
        public required string Status { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Email { get; set; }
    }
}
