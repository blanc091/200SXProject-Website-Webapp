namespace _200SXContact.Models.Configs
{
	public class ConnectionStrings
	{
		public string DefaultConnection { get; set; }
	}
	public class MicrosoftAuthentication
	{
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
	}
	public class EmailSettings
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}
	public class AdminSettings
	{
		public string Email { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}
	public class AppSettings
	{
		public ConnectionStrings ConnectionStrings { get; set; }
		public MicrosoftAuthentication Authentication { get; set; }
		public EmailSettings EmailSettings { get; set; }
		public AdminSettings AdminSettings { get; set; }
	}
}
