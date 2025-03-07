using _200SXContact.Models;
using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.Areas.Orders;

namespace _200SXContact.Interfaces.Areas.Admin
{
    public interface IEmailService
    {
        Task SendEmailToAdmin(ContactForm model);
        Task SendDueDateReminder(string userEmail, string userName, ReminderItem item, int daysBeforeDue);
        Task SendCommentNotification(string userEmail, BuildsComments comment);
        Task SendOrderConfirmEmail(string email, OrderPlacement order);
    }
}
