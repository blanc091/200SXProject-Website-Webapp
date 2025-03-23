using _200SXContact.Interfaces.Areas.Account;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.Account
{
	public class ResetPasswordViewModel : IResetPassword
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address !")]
		public required string Email { get; set; }
		public required string Token { get; set; }
		[Required(ErrorMessage = "New password is required.")]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		public required string NewPassword { get; set; }
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
		[Display(Name = "Confirm Password")]
		public required string ConfirmPassword { get; set; }
	}
}
