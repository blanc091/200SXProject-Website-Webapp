﻿using System.ComponentModel.DataAnnotations.Schema;

namespace _200SXContact.Models
{
	public class Item
	{
		public int Id { get; set; }
		public string EntryItem { get; set; }
		public string EntryDescription { get; set; }
		public DateTime DueDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string UserId { get; set; } // Foreign key to User
		public virtual User User { get; set; }
	}
}
