using _200SXContact.Models.DTOs.Areas.UserContent;

namespace _200SXContact.Models.DTOs.Areas.Account
{
    public class UserProfileDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool ShowDeleteConfirmation { get; set; } = false;
        public List<UserBuildDto> UserBuilds { get; set; } = new List<UserBuildDto>();
    }
}
