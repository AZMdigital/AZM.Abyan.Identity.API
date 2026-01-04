using System.ComponentModel.DataAnnotations;

namespace AZM.Abyan.Identity.Application.DTOs.Users;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
