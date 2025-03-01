namespace _200SXContact.Interfaces.Areas.Orders
{
    public interface IOrderTrackingUpdate
    {
        int Id { get; set; }
        int OrderId { get; set; }
        string Status { get; set; }
        string? Carrier { get; set; }
        string? TrackingNumber { get; set; }
        DateTime StatusUpdatedAt { get; set; }
        string? Email { get; set; }
        string? AddressLine { get; set; }
        string? OrderNotes { get; set; }
    }
}
