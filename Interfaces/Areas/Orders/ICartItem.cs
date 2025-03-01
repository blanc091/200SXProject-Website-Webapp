using _200SXContact.Models.Areas.Orders;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Interfaces.Areas.Orders
{
    public interface ICartItem
    {
         int Id { get; set; }
         int ProductId { get; set; }
         string ProductName { get; set; }
         decimal Price { get; set; }
         int Quantity { get; set; }
         string? ImagePath { get; set; }
         string? UserId { get; set; }
         int? OrderId { get; set; }
    }
}
