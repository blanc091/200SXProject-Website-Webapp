using _200SXContact.Models.DTOs.Areas.Orders;
using MediatR;
public class GetAllOrdersQuery : IRequest<List<OrderTrackingUpdateDto>> { }