using _200SXContact.Models.Areas.Orders;

namespace _200SXContact.Interfaces.Areas.Orders
{
    public interface IOrderPlacement
    {
        int Id { get; set; }
        string FullName { get; set; }
        string? AddressLine1 { get; set; }
        string? AddressLine2 { get; set; }
        string? City { get; set; }
        string? County { get; set; }
        string? PhoneNumber { get; set; }
        string Email { get; set; }
        string? OrderNotes { get; set; }
        DateTime OrderDate { get; set; }
        string? UserId { get; set; }
        string CartItemsJson { get; set; }
        List<CartItem> CartItems { get; set; }
    }
}
