namespace _200SXContact.Interfaces.Areas.Account
{
    public interface IRegister
    {
        string Username { get; set; }
        string Password { get; set; }
        string Email { get; set; }
        bool SubscribeToNewsletter { get; set; }
        string honeypotSpam { get; set; }
    }
}
