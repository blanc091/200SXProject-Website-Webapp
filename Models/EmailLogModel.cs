namespace _200SXContact.Models
{
    public class EmailLog
    {
        public int Id { get; set; }             
        public DateTime Timestamp { get; set; }
        public string ?From { get; set; }      
        public string ?To { get; set; }       
        public string ?Subject { get; set; }  
        public string ?Body { get; set; }   
        public string Status { get; set; } 
        public string ?ErrorMessage { get; set; }
    }
}
