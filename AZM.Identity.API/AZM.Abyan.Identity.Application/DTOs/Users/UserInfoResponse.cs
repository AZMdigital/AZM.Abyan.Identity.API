using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.DTOs.Users;

public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool EmailVerified { get; set; }
    public long CreatedTimestamp { get; set; }
    public List<RealmRoleResponse> RealmRoles { get; set; } = new();
    public Dictionary<string, List<ClientRoleResponse>> ClientRoles { get; set; } = new();
    public List<OrganizationSummary> Organizations { get; set; } = new();
}
public class OrganizationSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
public class OrganizationInfo
{
    public string id { get; set; } = string.Empty;
}
