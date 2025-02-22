using _200SXContact.Interfaces.Areas.Products;

namespace _200SXContact.Models.DTOs.Areas.Products
{
    public class ProductDto : IProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
		public List<string> ImagePaths { get; set; }
		public string AddedDate { get; set; }
    }
}