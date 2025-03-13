using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.Account
{
	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address!")]
		public string Email { get; set; }
	}
}