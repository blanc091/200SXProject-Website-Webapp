using _200SXContact.Interfaces.Areas.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Orders
{
    public class CartItem : ICartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required]
        public required string ProductName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required int Quantity { get; set; }
        public string? ImagePath { get; set; }
        public string? UserId { get; set; }
        public int? OrderId { get; set; }
        [ForeignKey("OrderPlacementId")]
        public virtual OrderPlacement? OrderPlacement { get; set; }
    }
}
