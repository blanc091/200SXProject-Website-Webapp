namespace _200SXContact.Models.DTOs.Areas.UserContent
{
    public class UserBuildDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string> ImagePaths { get; set; } = new List<string>();
        public DateTime? DateCreated { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public virtual Models.Areas.UserContent.User? User { get; set; }
        public virtual ICollection<BuildCommentDto>? Comments { get; set; } = new List<BuildCommentDto>();
    }
}
