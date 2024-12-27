using System.Threading.Tasks;

namespace _200SXContact.Interfaces
{
    public interface IDueDateReminderService
    {
        Task ManualCheckDueDates();
    }
}