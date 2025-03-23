using _200SXContact.Interfaces.Areas.MaintenApp;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Models.Areas.MaintenApp
{
	public class ReminderItem : IReminderItem
	{
		public int Id { get; set; }
		public required string EntryItem { get; set; }
		public required string EntryDescription { get; set; }
		public required DateTime DueDate { get; set; }
		public required DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
		public required string UserId { get; set; }
		public virtual User? User { get; set; }
		public bool EmailSent { get; set; }
	}
}
