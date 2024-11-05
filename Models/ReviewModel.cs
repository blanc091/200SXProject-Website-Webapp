using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class Review
	{
		public int Id { get; set; } 
		[Required]
		[ForeignKey("Product")]
		public int ProductId { get; set; }  
		[Required]
		public string UserId { get; set; } 
		[Required]
		[StringLength(1000)]
		public string Content { get; set; }  
		[Range(1, 5)]
		public int Rating { get; set; } 
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;	
		public virtual Product Product { get; set; } 
		public virtual User User { get; set; } 
	}
}
