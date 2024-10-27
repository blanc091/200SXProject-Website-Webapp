namespace _200SXContact.Models
{
	public class UserBuild
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImagePath { get; set; }
		public DateTime DateCreated { get; set; }
		public string UserEmail { get; set; }
		public string UserName { get; set; }
		public virtual User User { get; set; }
	}
}
