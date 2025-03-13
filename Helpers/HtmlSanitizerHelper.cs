using Ganss.Xss;

namespace _200SXContact.Helpers
{
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        public static string Sanitize(string input)
        {
            return _sanitizer.Sanitize(input);
        }
    }
}
