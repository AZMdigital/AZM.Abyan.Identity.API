namespace AZM.Abyan.Identity.Application.DTOs.Roles;

public class CreateRealmRoleRequest
{
    public string Realm { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
