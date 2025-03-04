using _200SXContact.Models.Areas.Orders;
using System.Text.Json;

namespace _200SXContact.Helpers.Areas.Orders
{
    public class CartItemsJsonHelper
    {
        public static string SerializeCartItems(List<CartItem> cartItems)
        {
            return JsonSerializer.Serialize(cartItems);
        }
        public static List<CartItem> DeserializeCartItems(string cartItemsJson)
        {
            return string.IsNullOrEmpty(cartItemsJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartItemsJson) ?? new List<CartItem>();
        }
    }
}
