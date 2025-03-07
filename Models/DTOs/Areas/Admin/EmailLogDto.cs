namespace _200SXContact.Models.DTOs.Areas.Admin
{
    public class EmailLogDto
    {
        public int Id { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string From { get; set; }
        public required string To { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
        public required string Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
