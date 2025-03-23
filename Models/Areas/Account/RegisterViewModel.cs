using _200SXContact.Interfaces.Areas.Account;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.Account
{
	public class RegisterViewModel : IRegister
	{
		[Required]
        [MaxLength(100, ErrorMessage = "Username must not exceed 100 characters !")]
		public required string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required bool SubscribeToNewsletter { get; set; }
		public string? honeypotSpam { get; set; }
	}
}
