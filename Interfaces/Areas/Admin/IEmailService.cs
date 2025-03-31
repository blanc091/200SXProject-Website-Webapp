using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Interfaces.Areas.Admin
{
    public interface IEmailService
    {
        Task SendEmailToAdmin(ContactForm model);
        Task SendDueDateReminder(string userEmail, string userName, ReminderItem item, int daysBeforeDue);
        Task SendCommentNotification(string userEmail, BuildComment comment);
        Task SendOrderConfirmEmail(string email, OrderPlacement order);
        Task NotifyNewChatSessionAsync(string sessionId);
        Task SendVerificationEmail(string email, string verificationUrl);
        Task SendPasswordResetEmail(string email, string resetUrl);
        Task SendUserDeleteEmail(string email, string userDeleteUrl);
        Task SendOrderUpdateEmail(string email, string orderUpdateUrl);
    }
}
