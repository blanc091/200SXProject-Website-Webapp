using System.ComponentModel.DataAnnotations;

namespace _200SXContact.Models.DTOs.Areas.Admin
{
    public class ContactFormDto
    {        
		[Required(ErrorMessage = "Name required.")]
        [StringLength(150, ErrorMessage = "Name cannot be longer than 150 characters.")]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Email required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Message required.")]
        [StringLength(10000, ErrorMessage = "Name cannot be longer than 10000 characters.")]
        public required string Message { get; set; }    
    }
}
