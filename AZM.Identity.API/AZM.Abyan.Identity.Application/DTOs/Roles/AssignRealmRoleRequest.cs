namespace AZM.Abyan.Identity.Application.DTOs.Roles;

public class AssignRealmRoleRequest
{
    public string Realm { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
