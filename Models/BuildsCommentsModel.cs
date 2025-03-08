namespace _200SXContact.Models
{
	public class BuildsComments
	{
		public int Id { get; set; }
		public required string Content { get; set; }
		public DateTime CreatedAt { get; set; }
		public required string UserId { get; set; }
		public required string UserName { get; set; }
		public required string UserBuildId { get; set; } 
		public required virtual UserBuild UserBuild { get; set; } 
	}
}
