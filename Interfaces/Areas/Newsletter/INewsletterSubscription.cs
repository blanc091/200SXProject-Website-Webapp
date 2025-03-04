namespace _200SXContact.Interfaces.Areas.Newsletter
{
    public interface INewsletterSubscription
    {
        int Id { get; set; }
        string Email { get; set; }
        bool IsSubscribed { get; set; }
        DateTime SubscribedAt { get; set; }
    }
}
