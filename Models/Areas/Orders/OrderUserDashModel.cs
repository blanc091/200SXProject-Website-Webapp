using _200SXContact.Interfaces.Areas.Orders;

namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderUserDash : IOrderUserDash
    {
        public List<OrderPlacementDto>? OrderPlacements { get; set; }
        public List<OrderTrackingDto>? OrderTrackings { get; set; }
    }
}
