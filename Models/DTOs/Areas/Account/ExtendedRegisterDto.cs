namespace _200SXContact.Models.DTOs.Areas.Account
{
    public class ExtendedRegisterDto : RegisterDto
    {
        public required string RecaptchaResponse { get; set; }
        public required string TimeZoneCookie { get; set; }
        public bool IsCalledFromRegisterForm { get; set; } = false;
    }
}
