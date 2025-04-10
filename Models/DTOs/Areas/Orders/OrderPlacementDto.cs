namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderPlacementDto
    {
        public required int Id { get; set;}
        public string? UserId { get; set; }
        public required string FullName { get; set; }
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public required string City { get; set; }
        public required string County { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public string? OrderNotes { get; set; }
        public required string CartItemsJson { get; set; }
        public required DateTime OrderDate { get; set; }
    }
}