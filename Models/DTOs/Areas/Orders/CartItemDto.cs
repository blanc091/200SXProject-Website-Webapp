using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Orders
{
    public class CartItemDto
    {
        public required int Id { get; set; }
        public required int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required decimal Price { get; set; }
        public required int Quantity { get; set; }
        public string? ImagePath { get; set; }
        public string? UserId { get; set; }
    }

}
