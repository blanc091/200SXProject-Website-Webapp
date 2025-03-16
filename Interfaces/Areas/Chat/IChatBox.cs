using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Interfaces.Areas.Chat
{
    public interface IChatBox
    {
        int Id { get; set; }
        string UserName { get; set; }
        string Message { get; set; }
        DateTime SentAt { get; set; }
    }
}
