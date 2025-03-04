namespace _200SXContact.Models
{
	public class BuildsComments
	{
		public int Id { get; set; }
		public string Content { get; set; }
		public DateTime CreatedAt { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserBuildId { get; set; } 
		public virtual UserBuild UserBuild { get; set; } 
	}
}
