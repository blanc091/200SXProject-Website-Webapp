using _200SXContact.Interfaces.Areas.Account;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Models.Areas.Account
{
	public class UserProfileViewModel : IUserProfile
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public bool ShowDeleteConfirmation { get; set; } = false;
		public List<UserBuild>? UserBuilds { get; set; }
	}
}
