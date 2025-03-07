using _200SXContact.Models.Areas.Admin;

namespace _200SXContact.Helpers.Areas.Admin
{
    public class ContactResult
    {
        public bool IsSuccess { get; }
        public string ViewName { get; }
        public ContactForm? Model { get; }
        public string Message { get; }
        private ContactResult(bool isSuccess, string viewName, ContactForm? model, string message)
        {
            IsSuccess = isSuccess;
            ViewName = viewName;
            Model = model;
            Message = message;
        }
        public static ContactResult Success(string viewName, string message) => new ContactResult(true, viewName, null, message);
        public static ContactResult Failure(string viewName, ContactForm model, string message) => new ContactResult(false, viewName, model, message);
    }
}
