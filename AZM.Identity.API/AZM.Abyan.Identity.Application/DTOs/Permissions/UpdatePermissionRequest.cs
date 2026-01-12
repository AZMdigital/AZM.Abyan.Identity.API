namespace AZM.Abyan.Identity.Application.DTOs.Permissions;

public class UpdatePermissionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? ScopeId { get; set; }
    public Guid? ResourceId { get; set; }
    public Guid? PolicyId { get; set; }
}

