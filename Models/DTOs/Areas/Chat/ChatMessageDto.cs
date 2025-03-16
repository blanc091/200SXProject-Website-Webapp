namespace _200SXContact.Models.DTOs.Areas.Chat
{
    public class ChatMessageDto
    {       
        public int Id { get; set; }
        public string? UserName { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
