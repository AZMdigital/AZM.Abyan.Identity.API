namespace AZM.Abyan.Identity.Application.DTOs.Permissions;

public class UpdatePermissionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    
    // Role-based permission model
    public string? Controller { get; set; } // Optional: Controller name
    public string? Action { get; set; } // Optional: Action name (can be set to null to remove)
}

