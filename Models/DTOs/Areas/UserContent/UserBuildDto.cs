using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.UserContent
{
    public class UserBuildDto
    {
        public required string Id { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Title must be at least 5 characters long.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public required string Title { get; set; }
        [Required]
        [MinLength(50, ErrorMessage = "Description must be at least 50 characters long.")]
        [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        public required string Description { get; set; }
        public List<string> ImagePaths { get; set; } = new List<string>();
        public required DateTime DateCreated { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public virtual Models.Areas.UserContent.User? User { get; set; }
        public virtual ICollection<BuildCommentDto>? Comments { get; set; } = new List<BuildCommentDto>();
    }
}
