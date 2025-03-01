using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;
using System.Security.Claims;

namespace _200SXContact.Commands.Areas.Orders
{
    public class PlaceOrderCommand(OrderPlacementDto model, ClaimsPrincipal user) : IRequest<int>
    {
        public OrderPlacementDto Model { get; } = model;
        public ClaimsPrincipal User { get; } = user;
    }
}
