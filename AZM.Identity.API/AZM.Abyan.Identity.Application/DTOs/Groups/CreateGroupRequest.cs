namespace AZM.Abyan.Identity.Application.DTOs.Groups;

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ParentGroupId { get; set; }
}
