namespace _200SXContact.Interfaces.Areas.Account
{
    public interface IResetPassword
    {
        string Email { get; set; }
        string Token { get; set; }
        string NewPassword { get; set; }
        string ConfirmPassword { get; set; }
    }
}
