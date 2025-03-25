using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Models.Areas.UserContent
{
    public class Review
	{
		public int Id { get; set; } 
		[Required]
		[ForeignKey("Product")]
		public int ProductId { get; set; }  
		[Required]
		public required string UserId { get; set; } 
		[Required]
		[StringLength(1000)]
		public required string Content { get; set; }  
		[Range(1, 5)]
		public int Rating { get; set; } 
		public DateTime CreatedAt { get; set; }
		public virtual Product Product { get; set; } 
		public virtual User User { get; set; } 
	}
}
