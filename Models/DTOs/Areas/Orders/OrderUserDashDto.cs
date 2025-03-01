namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderUserDashDto
    {
        public List<OrderPlacementDto> OrderPlacements { get; set; }
        public List<OrderTrackingDto> OrderTrackings { get; set; }
    }
}
