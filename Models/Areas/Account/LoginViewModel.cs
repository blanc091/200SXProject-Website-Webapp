using _200SXContact.Interfaces.Areas.Account;
using System.ComponentModel.DataAnnotations;
namespace _200SXContact.Models.Areas.Account
{	
	public class LoginViewModel : ILogin
    {
		[Required(ErrorMessage = "Username is required.")]
		public required string Username { get; set; }
		[Required(ErrorMessage = "Password is required.")]
		[DataType(DataType.Password)]
		public required string Password { get; set; }
	}	
}
