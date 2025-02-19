using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Products
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Category { get; set; }
        public required string Description { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public required decimal Price { get; set; }
        public List<string> ImagePaths { get; set; } = new List<string>();
        public DateTime DateAdded { get; set; }
    }
}
