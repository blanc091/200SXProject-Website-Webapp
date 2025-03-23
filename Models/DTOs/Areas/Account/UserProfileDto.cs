using _200SXContact.Models.DTOs.Areas.UserContent;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Account
{
    public class UserProfileDto
    {
        [Required]
        [MaxLength(100, ErrorMessage="Username cannot be longer than 100 characters.")]
        public required string UserName { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool ShowDeleteConfirmation { get; set; } = false;
        public List<UserBuildDto>? UserBuilds { get; set; } = new List<UserBuildDto>();
    }
}
