using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models
{
	public class ResetPasswordViewModel
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address!")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Token is required.")]
		public string Token { get; set; }

		[Required(ErrorMessage = "New password is required.")]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
		[Display(Name = "Confirm Password")]
		public string ConfirmPassword { get; set; }
	}
}
