using _200SXContact.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class User : IdentityUser
	{
		[Required(ErrorMessage = "Username is required.")]
		public required override string? UserName { get; set; }
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]		
		public required override string? Email { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public bool IsEmailVerified { get; set; }
		public string? EmailVerificationToken { get; set; }
		public string? PasswordResetToken { get; set; }
		public virtual ICollection<ReminderItem>? Items { get; set; }
	}
}


