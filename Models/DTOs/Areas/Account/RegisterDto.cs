namespace _200SXContact.Models.DTOs.Areas.Account
{
    public class RegisterDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public bool SubscribeToNewsletter { get; set; }
        public string? honeypotSpam { get; set; }
    }
}
