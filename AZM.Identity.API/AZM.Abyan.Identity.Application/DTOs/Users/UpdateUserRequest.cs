using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Users;

public class UpdateUserRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
