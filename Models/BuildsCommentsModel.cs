namespace _200SXContact.Models
{
	public class BuildsCommentsModel
	{
		public int Id { get; set; }
		public string Content { get; set; }
		public DateTime CreatedAt { get; set; }
		public string UserId { get; set; } // FK to User.Id
		public string UserName { get; set; } // To display the name of the user who posted the comment
		public string UserBuildId { get; set; } // FK to UserBuild.Id
		public virtual UserBuild UserBuild { get; set; } // Navigation property
	}
}
