using _200SXContact.Interfaces.Areas.Admin;

namespace _200SXContact.Models.Areas.Admin
{
	public class Logging : ILogging
	{
		public int Id { get; set; }
		public required DateTime Timestamp { get; set; }
		public required string LogLevel { get; set; }
		public required string Message { get; set; }
		public string Exception { get; set; }
	}
}
