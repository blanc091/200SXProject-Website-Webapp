using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Interfaces.Areas.Admin
{
    public interface IEmailService
    {
        Task SendEmailToAdminAsync(ContactForm model);
        Task SendDueDateReminderAsync(string userEmail, string userName, ReminderItem item, int daysBeforeDue);
        Task SendCommentNotificationAsync(string userEmail, BuildComment comment);
        Task SendOrderConfirmEmailAsync(string email, OrderPlacement order);
        Task NotifyNewChatSessionAsync(string sessionId);
        Task SendVerificationEmailAsync(string email, string verificationUrl);
        Task SendPasswordResetEmailAsync(string email, string resetUrl);
        Task SendUserDeleteEmailAsync(string email, string userDeleteUrl);
        Task SendOrderUpdateEmailAsync(string email, string orderUpdateUrl);
        Task SendEmailToSubscriberAsync(string email, string subject, string body);
    }
}
