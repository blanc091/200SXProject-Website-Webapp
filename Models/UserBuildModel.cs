namespace _200SXContact.Models
{
	public class UserBuild
	{
		public string? Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public List<string> ImagePaths { get; set; } = new List<string>();
		public DateTime? DateCreated { get; set; }
		public string? UserEmail { get; set; }
		public string? UserName { get; set; }
		public string? UserId { get; set; } 
		public virtual User? User { get; set; }
		public virtual ICollection<BuildsComments> Comments { get; set; } = new List<BuildsComments>();
	}
}
