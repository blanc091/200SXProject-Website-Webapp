using _200SXContact.Queries.Areas.Admin;

namespace _200SXContact.Helpers
{
    public static class ClientTimeHelper
    {
        public static DateTime GetCurrentClientTime(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor?.HttpContext?.Request == null)
            {
                throw new InvalidOperationException("HTTP request is not available.");
            }

            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            GetClientTime clientTimeService = new GetClientTime();
            DateTime currentClientTime = clientTimeService.GetClientTimeHandler(httpRequest);

            return currentClientTime;
        }
    }
}
