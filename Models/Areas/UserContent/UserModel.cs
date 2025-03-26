using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.Orders;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.UserContent
{
	public class User : IdentityUser
	{
		[Required(ErrorMessage = "Username is required.")]
		public required override string? UserName { get; set; }
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]		
		public required override string? Email { get; set; }
		public required DateTime CreatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public required bool IsEmailVerified { get; set; }
		public string? EmailVerificationToken { get; set; }
		public string? PasswordResetToken { get; set; }
		public virtual ICollection<ReminderItem>? Items { get; set; }
        public ICollection<OrderPlacement> Orders { get; set; } = new List<OrderPlacement>();
        public virtual ICollection<UserBuild>? UserBuilds { get; set; } = new List<UserBuild>();
    }
}


