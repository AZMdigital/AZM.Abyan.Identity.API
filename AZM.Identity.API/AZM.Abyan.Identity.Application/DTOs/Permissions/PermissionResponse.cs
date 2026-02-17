namespace AZM.Abyan.Identity.Application.DTOs.Permissions;

public class PermissionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Role-based permission model
    public required string Controller { get; set; } // Mandatory: Controller name
    public string? Action { get; set; } // Optional: Action name

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

