using _200SXContact.Interfaces.Areas.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Orders
{
    public class CartItem : ICartItem
    {
        public required int Id { get; set; }
        public required int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required decimal Price { get; set; }
        public required int Quantity { get; set; }
        public string? ImagePath { get; set; }
        public string? UserId { get; set; }
        public int? OrderId { get; set; }
        public virtual OrderPlacement? OrderPlacement { get; set; }
    }
}
