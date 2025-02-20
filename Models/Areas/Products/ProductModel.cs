using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Products
{
    public class Product
    {
        public int Id { get; set; }
		[Required(ErrorMessage = "Product Name is required.")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Category is required.")]
		public string Category { get; set; }
		[Required(ErrorMessage = "Description is required.")]
		public string Description { get; set; }
		[Column(TypeName = "decimal(6,2)"), Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99.")]
        public required decimal Price { get; set; }
        public List<string> ImagePaths { get; set; } = new List<string>();
        public DateTime DateAdded { get; set; }
    }
}
