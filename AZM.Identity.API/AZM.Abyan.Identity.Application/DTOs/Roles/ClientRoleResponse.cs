namespace AZM.Abyan.Identity.Application.DTOs.Roles;

public class ClientRoleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Composite { get; set; }
    public bool ClientRole { get; set; }
    public string ContainerId { get; set; } = string.Empty;
}

