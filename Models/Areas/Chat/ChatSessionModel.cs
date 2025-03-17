using _200SXContact.Interfaces.Areas.Chat;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.Chat
{
    public class ChatSession : IChatSession
    {
        [Key]
        public required string SessionId { get; set; }
        public required string ConnectionId { get; set; }
        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public bool IsAnswered { get; set; } = false;
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
    }
}
