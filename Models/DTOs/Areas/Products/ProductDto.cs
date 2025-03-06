using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.DTOs.Areas.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Product Name is required.")]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Category is required.")]
        public required string Category { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public required string Description { get; set; }
        [Column(TypeName = "decimal(6,2)"), Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99.")]
        public required decimal Price { get; set; }
        public List<string> ImagePaths { get; set; }
        [Required]
        public required string AddedDate { get; set; }
    }
}