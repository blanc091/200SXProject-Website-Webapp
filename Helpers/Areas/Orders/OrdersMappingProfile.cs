using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using _200SXContact.Helpers.Areas.Orders;
using _200SXContact.Helpers;

public class OrdersMappingProfile : Profile
{
    public OrdersMappingProfile()
    {
        CreateMap<OrderPlacementDto, OrderPlacement>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.AddressLine1))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.AddressLine2))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.County))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.OrderNotes, opt => opt.MapFrom(src => src.OrderNotes))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.CartItemsJson, opt => opt.MapFrom(src => src.CartItemsJson))
            .ReverseMap();        

        CreateMap<OrderPlacement, OrderTrackingDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AddressLine, opt => opt.MapFrom<AddressResolver>())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StatusUpdatedAt, opt => opt.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.Carrier, opt => opt.MapFrom(src => src.OrderTracking != null ? src.OrderTracking.Carrier : null))
            .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.OrderTracking != null ? src.OrderTracking.TrackingNumber : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.OrderNotes, opt => opt.MapFrom(src => src.OrderNotes))
            .ForMember(dest => dest.CartItemsJson, opt => opt.MapFrom(src => src.CartItemsJson));

        CreateMap<OrderTrackingDto, OrderPlacement>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => new ReverseAddressResolver().Resolve(src, null, null, null)[0]))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => new ReverseAddressResolver().Resolve(src, null, null, null)[1]))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.StatusUpdatedAt))
            .ForMember(dest => dest.OrderTracking, opt => opt.Ignore())
            .ForMember(dest => dest.CartItemsJson, opt => opt.MapFrom(src => src.CartItemsJson));

        CreateMap<OrderTrackingUpdateDto, OrderTracking>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Carrier, opt => opt.MapFrom(src => src.Carrier))
            .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
            .ForMember(dest => dest.StatusUpdatedAt, opt => opt.MapFrom<ClientTimeResolver<OrderTrackingUpdateDto, OrderTracking>>());

        CreateMap<OrderTracking, OrderTrackingUpdateDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Carrier, opt => opt.MapFrom(src => src.Carrier))
            .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber));

        CreateMap<OrderTrackingDto, OrderTracking>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Carrier, opt => opt.MapFrom(src => src.Carrier))
            .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
            .ForMember(dest => dest.StatusUpdatedAt, opt => opt.MapFrom(src => src.StatusUpdatedAt))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.AddressLine, opt => opt.MapFrom(src => src.AddressLine))
            .ForMember(dest => dest.OrderNotes, opt => opt.MapFrom(src => src.OrderNotes))
            .ForMember(dest => dest.CartItemsJson, opt => opt.MapFrom(src => src.CartItemsJson))
            .ReverseMap();

        CreateMap<OrderTrackingUpdate, OrderTracking>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Carrier, opt => opt.MapFrom(src => src.Carrier))
            .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
            .ForMember(dest => dest.StatusUpdatedAt, opt => opt.MapFrom(src => src.StatusUpdatedAt))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.AddressLine, opt => opt.MapFrom(src => src.AddressLine))
            .ForMember(dest => dest.OrderNotes, opt => opt.MapFrom(src => src.OrderNotes))
            .ReverseMap();

        CreateMap<OrderUserDashDto, OrderUserDash>()
            .ForMember(dest => dest.OrderPlacements, opt => opt.MapFrom(src => src.OrderPlacements))
            .ForMember(dest => dest.OrderTrackings, opt => opt.MapFrom(src => src.OrderTrackings))
            .ReverseMap();

        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ReverseMap();

        CreateMap<OrderTrackingDto, OrderTrackingUpdateDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Carrier, opt => opt.MapFrom(src => src.Carrier))
            .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
            .ReverseMap();
    }
}
