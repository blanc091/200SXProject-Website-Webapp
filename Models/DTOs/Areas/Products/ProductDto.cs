using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Category { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public required decimal Price { get; set; }
		public List<string> ImagePaths { get; set; }
        [Required]
        public required string AddedDate { get; set; }
    }
}