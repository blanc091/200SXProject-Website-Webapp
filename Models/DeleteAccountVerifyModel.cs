using System.ComponentModel.DataAnnotations;
public class DeleteAccountVerifyModel
{
	[Required]
	[EmailAddress]
	public string UserEmail { get; set; }
}