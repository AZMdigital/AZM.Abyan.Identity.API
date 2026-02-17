namespace AZM.Abyan.Identity.Application.DTOs.Organizations;

public class CreateOrganizationRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
