using AZM.Identity.Application.DTOs.Roles;

namespace AZM.Identity.Application.Services;

public class RoleService : IRoleService
{
    private readonly IKeycloakService _keycloakService;

    public RoleService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<List<ClientRoleResponse>> GetClientRolesAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetClientRolesAsync(clientId, adminToken, cancellationToken);
    }

    public async Task AssignClientRoleToUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.AssignClientRoleToUserAsync(request.UserId, request.ClientId, request.RoleName, adminToken, cancellationToken);
    }

    public async Task RemoveClientRoleFromUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.RemoveClientRoleFromUserAsync(request.UserId, request.ClientId, request.RoleName, adminToken, cancellationToken);
    }
}

