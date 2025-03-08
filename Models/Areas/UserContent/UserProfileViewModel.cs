namespace _200SXContact.Models.Areas.UserContent
{
	public class UserProfileViewModel
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public bool ShowDeleteConfirmation { get; set; } = false;
		public List<UserBuild> UserBuilds { get; set; }
	}
}
