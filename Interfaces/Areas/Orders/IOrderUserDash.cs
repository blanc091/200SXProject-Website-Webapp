using _200SXContact.Models.DTOs.Areas.Orders;

namespace _200SXContact.Interfaces.Areas.Orders
{
    public interface IOrderUserDash
    {
        List<OrderPlacementDto> OrderPlacements { get; set; }
        List<OrderTrackingDto> OrderTrackings { get; set; }
    }
}
