using _200SXContact.Interfaces.Areas.Newsletter;

namespace _200SXContact.Models.Areas.Newsletter
{
	public class NewsletterViewModel : INewsletter
	{
		public required string Subject { get; set; }
		public required string Body { get; set; }
	}
}