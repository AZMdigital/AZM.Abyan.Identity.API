using AZM.Identity.Application.DTOs.Groups;

namespace AZM.Identity.Application.Services;

public interface IGroupService
{
    Task<List<GroupResponse>> GetGroupsAsync(CancellationToken cancellationToken = default);
    Task AddUserToGroupAsync(AddUserToGroupRequest request, CancellationToken cancellationToken = default);
}

