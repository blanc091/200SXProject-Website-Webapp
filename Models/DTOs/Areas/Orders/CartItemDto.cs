using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required]
        public required string ProductName { get; set; }
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required int Quantity { get; set; }
        public string ImagePath { get; set; }
        public string UserId { get; set; }
    }

}
