using _200SXContact.Models.Areas.Chat;

namespace _200SXContact.Models.DTOs.Areas.Chat
{
    public class ChatSessionDto
    {
        public required string SessionId { get; set; }
        public required string ConnectionId { get; set; }
        public string? UserName { get; set; }
        public bool IsAnswered { get; set; } = false;
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
    }
}
