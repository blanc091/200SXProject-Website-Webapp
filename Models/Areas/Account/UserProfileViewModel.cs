using _200SXContact.Interfaces.Areas.Account;
using _200SXContact.Models.Areas.UserContent;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.Account
{
	public class UserProfileViewModel : IUserProfile
	{
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required DateTime CreatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public bool ShowDeleteConfirmation { get; set; } = false;
		public List<UserBuild>? UserBuilds { get; set; }
	}
}
