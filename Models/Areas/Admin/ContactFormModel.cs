using _200SXContact.Interfaces.Areas.Admin;

namespace _200SXContact.Models.Areas.Admin
{
    public class ContactForm : IContactForm
    {
		public required string Name { get; set; }
		public required string Email { get; set; }
		public required string Message { get; set; }
	}
}
