using AZM.Identity.Application.DTOs.Groups;

namespace AZM.Identity.Application.Services;

public class GroupService : IGroupService
{
    private readonly IKeycloakService _keycloakService;

    public GroupService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<List<GroupResponse>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetGroupsAsync(adminToken, cancellationToken);
    }

    public async Task AddUserToGroupAsync(AddUserToGroupRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.AddUserToGroupAsync(request.UserId, request.GroupId, adminToken, cancellationToken);
    }
}

