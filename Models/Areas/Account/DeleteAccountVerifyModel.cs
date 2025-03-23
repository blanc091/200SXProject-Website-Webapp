using _200SXContact.Interfaces.Areas.Account;
using System.ComponentModel.DataAnnotations;
public class DeleteAccountVerifyModel : IDeleteAccountVerify
{
	[Required]
	[EmailAddress]
	public required string UserEmail { get; set; }
}