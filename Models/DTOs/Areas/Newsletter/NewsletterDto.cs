using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Newsletter
{
    public class NewsletterDto
    {
        [Required]
        public required string Subject { get; set; }
        [Required]
        public required string Body { get; set; }
    }
}
