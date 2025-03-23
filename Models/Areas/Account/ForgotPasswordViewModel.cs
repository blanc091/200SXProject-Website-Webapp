using _200SXContact.Interfaces.Areas.Account;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.Account
{
	public class ForgotPasswordViewModel : IForgotPassword
    {
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address!")]
		public required string Email { get; set; }
	}
}