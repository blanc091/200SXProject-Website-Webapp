using _200SXContact.Interfaces.Areas.Account;

namespace _200SXContact.Models.Areas.Account
{
	public class RegisterViewModel : IRegister
	{
		public required string Username { get; set; }
		public required string Password { get; set; }
		public required string Email { get; set; }
		public bool SubscribeToNewsletter { get; set; }
		public string honeypotSpam { get; set; }
	}
}
