using _200SXContact.Interfaces.Areas.Admin;

namespace _200SXContact.Models.Areas.Admin
{
    public class EmailLog : IEmailLog
    {
        public int Id { get; set; }             
        public required DateTime Timestamp { get; set; }
        public required string From { get; set; }      
        public required string To { get; set; }       
        public required string Subject { get; set; }  
        public required string Body { get; set; }   
        public required string Status { get; set; } 
        public string ?ErrorMessage { get; set; }
    }
}