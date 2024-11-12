using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public string Description { get; set; }
		[Column(TypeName = "decimal(4,2)")]
		public decimal Price { get; set; }
		public List<string> ImagePaths { get; set; } = new List<string>();
		public DateTime DateAdded { get; set; }
	}
}
