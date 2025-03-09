using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Interfaces.Areas.UserContent
{
	public interface IBuildComment
	{
        int Id { get; set; }
        string Content { get; set; }
        DateTime CreatedAt { get; set; }
        string UserId { get; set; }
        string UserName { get; set; }
        string UserBuildId { get; set; }
        abstract UserBuild UserBuild { get; set; }
    }
}