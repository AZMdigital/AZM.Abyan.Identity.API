namespace AZM.Abyan.Identity.Application.DTOs.Users;

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
    //public bool Temporary { get; set; } = false;
}
