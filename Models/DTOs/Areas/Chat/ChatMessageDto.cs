using _200SXContact.Models.Areas.Chat;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.DTOs.Areas.Chat
{
    public class ChatMessageDto
    {       
        public int Id { get; set; }
        public string? UserName { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string SessionId { get; set; } = null!;
        public virtual ChatSession? Session { get; set; }
    }
}
