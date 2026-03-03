namespace AZM.Abyan.Identity.Application.DTOs.Permissions;

public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ScopeId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid PolicyId { get; set; }
}

