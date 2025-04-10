﻿namespace _200SXContact.Models.DTOs.Areas.UserContent
{
    public class BuildCommentDto
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string UserBuildId { get; set; }
        public virtual Models.Areas.UserContent.UserBuild? UserBuild { get; set; }
    }
}
