using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.Users;

namespace AZM.Abyan.Identity.Application.Services;

public interface IGroupService
{
    Task<List<GroupResponse>> GetGroupsAsync(CancellationToken cancellationToken = default);
    Task<GroupResponse?> GetGroupByIdAsync(string groupId, CancellationToken cancellationToken = default);
    Task CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken = default);
    Task UpdateGroupAsync(string groupId, UpdateGroupRequest request, CancellationToken cancellationToken = default);
    Task DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetGroupMembersAsync(string groupId, CancellationToken cancellationToken = default);
    Task AddUserToGroupAsync(AddUserToGroupRequest request, CancellationToken cancellationToken = default);
    Task RemoveUserFromGroupAsync(string userId, string groupId, CancellationToken cancellationToken = default);
}

