namespace AZM.Abyan.Identity.Application.DTOs.Roles;

public class UpdateClientRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Custom attributes for role-based permissions
    public Dictionary<string, string[]>? Attributes { get; set; }
}
