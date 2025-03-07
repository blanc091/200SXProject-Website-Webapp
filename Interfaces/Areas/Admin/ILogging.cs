namespace _200SXContact.Interfaces.Areas.Admin
{
    public interface ILogging
    {
        int Id { get; set; }
        DateTime Timestamp { get; set; }
        string LogLevel { get; set; }
        string Message { get; set; }
        string Exception { get; set; }
    }
}
