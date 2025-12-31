namespace AZM.Identity.Application.DTOs.Roles;

public class ClientRoleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Composite { get; set; }
    public string ClientRole { get; set; } = string.Empty;
    public string ContainerId { get; set; } = string.Empty;
}

