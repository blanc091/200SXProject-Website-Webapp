using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Interfaces.Areas.MaintenApp
{
    public interface IReminderItem
    {
        int Id { get; set; }
        string EntryItem { get; set; }
        string EntryDescription { get; set; }
        DateTime DueDate { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        string UserId { get; set; }
        User User { get; set; }
        bool EmailSent { get; set; }
    }
}
