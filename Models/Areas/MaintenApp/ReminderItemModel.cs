﻿using _200SXContact.Interfaces.Areas.MaintenApp;
using _200SXContact.Models.Areas.UserContent;

namespace _200SXContact.Models.Areas.MaintenApp
{
	public class ReminderItem : IReminderItem
	{
		public int Id { get; set; }
		public string EntryItem { get; set; }
		public string EntryDescription { get; set; }
		public DateTime DueDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string UserId { get; set; }
		public virtual User User { get; set; }
		public bool EmailSent { get; set; }
	}
}
