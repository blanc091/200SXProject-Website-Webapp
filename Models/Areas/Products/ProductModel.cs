using _200SXContact.Interfaces.Areas.Products;

namespace _200SXContact.Models.Areas.Products
{
    public class Product : IProduct
    {
        public required int Id { get; set; }
		public required string Name { get; set; }
		public required string Category { get; set; }
		public required string Description { get; set; }
        public required decimal Price { get; set; }
        public List<string>? ImagePaths { get; set; }
        public required DateTime DateAdded { get; set; }
    }
}
