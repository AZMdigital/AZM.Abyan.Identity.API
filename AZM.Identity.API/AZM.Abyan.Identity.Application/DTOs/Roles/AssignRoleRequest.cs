using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Roles;

public class AssignRoleRequest
{
    public string UserId { get; set; } = string.Empty;
    [JsonIgnore]
    public string ClientId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

