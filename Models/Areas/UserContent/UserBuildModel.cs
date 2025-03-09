using _200SXContact.Interfaces.Areas.UserContent;

namespace _200SXContact.Models.Areas.UserContent
{
	public class UserBuild : IUserBuild
	{
		public string? Id { get; set; }
		public required string Title { get; set; }
		public required string Description { get; set; }
		public List<string> ImagePaths { get; set; } = new List<string>();
		public required DateTime? DateCreated { get; set; }
		public required string? UserEmail { get; set; }
		public required string? UserName { get; set; }
		public required string? UserId { get; set; } 
		public virtual User? User { get; set; }
		public virtual ICollection<BuildComment> Comments { get; set; } = new List<BuildComment>();
	}
}
