using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Roles;

public class CreateClientRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Custom attributes for role-based permissions
    [JsonIgnore]
    public Dictionary<string, string[]>? Attributes { get; set; }
}
