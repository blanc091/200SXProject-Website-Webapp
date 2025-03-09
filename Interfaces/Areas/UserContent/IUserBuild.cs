using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Interfaces.Areas.UserContent
{
    public interface IUserBuild
    {
        string? Id { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        List<string> ImagePaths { get; set; }
        DateTime? DateCreated { get; set; }
        string? UserEmail { get; set; }
        string? UserName { get; set; }
        string? UserId { get; set; }
        User? User { get; set; }
        ICollection<BuildComment> Comments { get; set; }
    }
}
