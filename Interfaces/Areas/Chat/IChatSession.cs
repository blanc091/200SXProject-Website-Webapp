namespace _200SXContact.Interfaces.Areas.Chat
{
    public interface IChatSession
    {
        string SessionId { get; set; }
        string ConnectionId { get; set; }
        string? UserName { get; set; }
        bool IsAnswered { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? LastUpdatedAt { get; set; }
    }
}
