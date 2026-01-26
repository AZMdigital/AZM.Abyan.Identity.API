using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class Permission : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Role-based permission model
    // Permission is created as a role in Keycloak with custom attributes
    public required string Controller { get; set; } // Mandatory: Controller name
    public string? Action { get; set; } // Optional: Action name
    
    // Note: Permission is stored as a role in Keycloak, so we reference the Role entity
    // The role ID in Keycloak is the same as Permission ID
}
