using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models
{
	public class NewsletterSubscription
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public bool IsSubscribed { get; set; }
		public DateTime SubscribedAt { get; set; }
	}
}
