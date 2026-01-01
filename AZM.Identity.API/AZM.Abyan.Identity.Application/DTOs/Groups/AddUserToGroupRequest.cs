namespace AZM.Abyan.Identity.Application.DTOs.Groups;

public class AddUserToGroupRequest
{
    public string UserId { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
}

