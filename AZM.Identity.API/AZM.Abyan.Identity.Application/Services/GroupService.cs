namespace AZM.Abyan.Identity.Application.Services;

//public class GroupService : IGroupService
//{
//    private readonly IKeycloakService _keycloakService;

//    public GroupService(IKeycloakService keycloakService)
//    {
//        _keycloakService = keycloakService;
//    }

//    public async Task<List<GroupResponse>> GetGroupsAsync(CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        return await _keycloakService.GetGroupsAsync(adminToken, cancellationToken);
//    }

//    public async Task<GroupResponse?> GetGroupByIdAsync(string groupId, CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        return await _keycloakService.GetGroupByIdAsync(groupId, adminToken, cancellationToken);
//    }

//    public async Task CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        await _keycloakService.CreateGroupAsync(request, adminToken, cancellationToken);
//    }

//    public async Task UpdateGroupAsync(string groupId, UpdateGroupRequest request, CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        await _keycloakService.UpdateGroupAsync(groupId, request, adminToken, cancellationToken);
//    }

//    public async Task DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        await _keycloakService.DeleteGroupAsync(groupId, adminToken, cancellationToken);
//    }

//    public async Task<List<UserResponse>> GetGroupMembersAsync(string groupId, CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        return await _keycloakService.GetGroupMembersAsync(groupId, adminToken, cancellationToken);
//    }

//    public async Task AddUserToGroupAsync(AddUserToGroupRequest request, CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        await _keycloakService.AddUserToGroupAsync(request.UserId, request.GroupId, adminToken, cancellationToken);
//    }

//    public async Task RemoveUserFromGroupAsync(string userId, string groupId, CancellationToken cancellationToken = default)
//    {
//        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
//        await _keycloakService.RemoveUserFromGroupAsync(userId, groupId, adminToken, cancellationToken);
//    }
//}

