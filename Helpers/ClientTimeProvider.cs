using _200SXContact.Interfaces;

namespace _200SXContact.Helpers
{
    public class ClientTimeProvider : IClientTimeProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClientTimeProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public DateTime GetCurrentClientTime()
        {
            return ClientTimeHelper.GetCurrentClientTime(_httpContextAccessor);
        }
    }
}
