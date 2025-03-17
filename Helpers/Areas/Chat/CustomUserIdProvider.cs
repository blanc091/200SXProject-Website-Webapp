using Microsoft.AspNetCore.SignalR;

namespace _200SXContact.Helpers.Areas.Chat
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Identity?.Name;
        }
    }
}
