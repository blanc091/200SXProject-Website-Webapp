namespace _200SXContact.Models.Areas.Chat
{
    public class ChatSession
    {
        public required string SessionId { get; set; }
        public required string ConnectionId { get; set; }
        public string? UserName { get; set; }
    }
}
