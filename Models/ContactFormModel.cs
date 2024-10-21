using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models
{
    public class ContactForm
    {
		[Required(ErrorMessage = "Name required.")]
		[StringLength(150, ErrorMessage = "Name cannot be longer than 150 characters.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Email required.")]
		[EmailAddress(ErrorMessage = "Please enter a valid email address.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Message required.")]
		[StringLength(10000, ErrorMessage = "Name cannot be longer than 10000 characters.")]
		public string Message { get; set; }
	}
}
