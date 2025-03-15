using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Interfaces.Areas.Account
{
    public interface IUserProfile
    {
        string UserName { get; set; }
        string Email { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? LastLogin { get; set; }
        bool ShowDeleteConfirmation { get; set; }
        List<UserBuild> UserBuilds { get; set; }
    }
}
