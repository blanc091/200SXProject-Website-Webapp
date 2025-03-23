using _200SXContact.Interfaces.Areas.UserContent;

namespace _200SXContact.Models.Areas.UserContent
{
    public class BuildComment : IBuildComment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string UserBuildId { get; set; }
        public virtual UserBuild? UserBuild { get; set; }       
	}
}