using _200SXContact.Models.Areas.Orders;
using System.Text.Json;

namespace _200SXContact.Helpers.Areas.Orders
{
    public class CartItemsJsonHelper
    {
        public static string SerializeCartItems(List<CartItemModel> cartItems)
        {
            return JsonSerializer.Serialize(cartItems);
        }
        public static List<CartItemModel> DeserializeCartItems(string cartItemsJson)
        {
            return string.IsNullOrEmpty(cartItemsJson) ? new List<CartItemModel>() : JsonSerializer.Deserialize<List<CartItemModel>>(cartItemsJson) ?? new List<CartItemModel>();
        }
    }
}
