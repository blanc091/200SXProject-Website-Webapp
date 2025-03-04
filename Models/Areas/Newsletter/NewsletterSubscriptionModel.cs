using _200SXContact.Interfaces.Areas.Newsletter;

namespace _200SXContact.Models.Areas.Newsletter
{
	public class NewsletterSubscription : INewsletterSubscription
	{
		public int Id { get; set; }
		public required string Email { get; set; }
		public bool IsSubscribed { get; set; }
		public DateTime SubscribedAt { get; set; }
	}
}
