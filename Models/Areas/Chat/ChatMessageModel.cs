using _200SXContact.Interfaces.Areas.Chat;
using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.Areas.Chat
{
    public class ChatMessage : IChatBox
    {
        [Key]
        public int Id { get; set; }
        public string? UserName { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
