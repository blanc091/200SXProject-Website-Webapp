namespace _200SXContact.Models.DTOs.Areas.Admin
{
    public class LoggingDto
    {
        public int Id { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string LogLevel { get; set; }
        public required string Message { get; set; }
        public string Exception { get; set; }
    }
}
