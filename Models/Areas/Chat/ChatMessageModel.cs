using _200SXContact.Interfaces.Areas.Chat;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models.Areas.Chat
{
    public class ChatMessage : IChatBox
    {
        [Key]
        public int Id { get; set; }
        public string SessionId { get; set; } = null!;
        [ForeignKey(nameof(SessionId))]
        public virtual ChatSession? Session { get; set; }
        public string? UserName { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; }
    }
}
