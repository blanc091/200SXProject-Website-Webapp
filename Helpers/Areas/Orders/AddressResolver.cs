using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;

namespace _200SXContact.Helpers.Areas.Orders
{
    public class AddressResolver : IValueResolver<OrderPlacement, OrderTrackingDto, string>
    {
        public string Resolve(OrderPlacement source, OrderTrackingDto destination, string destMember, ResolutionContext context)
        {
            return string.Join(", ", new[] { source.AddressLine1, source.AddressLine2 }.Where(s => !string.IsNullOrEmpty(s)));
        }
    }

    public class ReverseAddressResolver : IValueResolver<OrderTrackingDto, OrderPlacement, string[]>
    {
        public string[] Resolve(OrderTrackingDto source, OrderPlacement destination, string[] destMember, ResolutionContext context)
        {
            var addressParts = source.AddressLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return new[]
            {
            addressParts.Length > 0 ? addressParts[0].Trim() : null,
            addressParts.Length > 1 ? addressParts[1].Trim() : null
        };
        }
    }
}
