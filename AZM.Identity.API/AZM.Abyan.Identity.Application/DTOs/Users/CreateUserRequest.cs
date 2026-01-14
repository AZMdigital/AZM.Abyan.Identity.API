using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Users;

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    [JsonIgnore]
    public bool Enabled { get; set; } = true;
    [JsonIgnore]
    public bool EmailVerified { get; set; } = false;
    public string? RealmName { get; set; } // Realm name to identify which realm the user belongs to
}

