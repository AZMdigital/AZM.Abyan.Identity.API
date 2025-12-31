namespace AZM.Identity.Application.DTOs.Roles;

public class AssignRoleRequest
{
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

