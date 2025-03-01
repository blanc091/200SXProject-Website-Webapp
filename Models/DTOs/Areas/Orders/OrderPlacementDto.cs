using System.ComponentModel.DataAnnotations;
namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderPlacementDto
    {
        public int Id { get; set;}
        public string? UserId { get; set; }
        [Required]
        public required string FullName { get; set; }
        [Required]
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? County { get; set; }
        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        public string? OrderNotes { get; set; }
        [Required]
        public required string CartItemsJson { get; set; }
        [Required]
        public required DateTime OrderDate { get; set; }
    }
}