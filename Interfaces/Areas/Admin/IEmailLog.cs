namespace _200SXContact.Interfaces.Areas.Admin
{
    public interface IEmailLog
    {
        int Id { get; set; }
        DateTime Timestamp { get; set; }
        string From { get; set; }
        string To { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        string Status { get; set; }
        string? ErrorMessage { get; set; }
    }
}
