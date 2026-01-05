using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class Permission : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    // Format: api:{controller}:{action}
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    
    // Keycloak mapping fields
    public string? KeycloakResourceId { get; set; }
    public string? KeycloakScopeId { get; set; }
    public string? KeycloakPermissionId { get; set; }
    public bool Synced { get; set; } = false;
}
