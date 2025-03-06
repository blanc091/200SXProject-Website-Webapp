using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Newsletter
{
    public class NewsletterSubscriptionDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public required string Email { get; set; }
        public bool IsSubscribed { get; set; }
        public DateTime SubscribedAt { get; set; }
    }
}
