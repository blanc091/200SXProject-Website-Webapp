namespace _200SXContact.Queries.Areas.Admin
{
	public class GetClientTime
	{
		public DateTime GetClientTimeHandler(HttpRequest request)
		{
			string? userTimeZone = request.Cookies["userTimeZone"];

            if (!string.IsNullOrEmpty(userTimeZone))
            {
                try
                {
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
                }
                catch (TimeZoneNotFoundException)
                {
                    return DateTime.UtcNow;
                }
                catch (InvalidTimeZoneException)
                {
                    return DateTime.UtcNow;
                }
            }
            else
            {
                return DateTime.UtcNow;
            }
        }
	}
}
