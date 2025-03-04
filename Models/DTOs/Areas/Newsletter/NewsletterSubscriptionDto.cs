namespace _200SXContact.Models.DTOs.Areas.Newsletter
{
    public class NewsletterSubscriptionDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public bool IsSubscribed { get; set; }
        public DateTime SubscribedAt { get; set; }
    }
}
