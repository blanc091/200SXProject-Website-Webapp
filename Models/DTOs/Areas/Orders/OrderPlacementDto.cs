using System.ComponentModel.DataAnnotations;
namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class OrderPlacementDto
    {
        public required int Id { get; set;}
        public string? UserId { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Full Name cannot exceed 50 characters.")]
        public required string FullName { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "The city cannot exceed 50 characters.")]
        public required string City { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "The country cannot exceed 50 characters.")]
        public required string County { get; set; }
        [Required]
        [Phone]
        [MaxLength(20, ErrorMessage = "The phone number cannot exceed 20 characters.")]
        public required string PhoneNumber { get; set; }
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