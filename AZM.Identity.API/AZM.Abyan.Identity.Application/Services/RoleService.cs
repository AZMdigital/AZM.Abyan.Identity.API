using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public class RoleService : IRoleService
{
    private readonly IKeycloakService _keycloakService;

    public RoleService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<List<ClientRoleResponse>> GetClientRolesAsync(string realm, string clientId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetClientRolesAsync(realm, clientId, adminToken, cancellationToken);
    }

    public async Task CreateClientRoleAsync(string realm, string clientId, CreateClientRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.CreateClientRoleAsync(realm, clientId, request, adminToken, cancellationToken);
    }

    public async Task DeleteClientRoleAsync(string realm, string clientId, string roleName, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteClientRoleAsync(realm, clientId, roleName, adminToken, cancellationToken);
    }

    public async Task AssignClientRoleToUserAsync(string realm, AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.AssignClientRoleToUserAsync(realm, request.UserId, request.ClientId, request.RoleName, adminToken, cancellationToken);
    }

    public async Task RemoveClientRoleFromUserAsync(string realm, AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.RemoveClientRoleFromUserAsync(realm, request.UserId, request.ClientId, request.RoleName, adminToken, cancellationToken);
    }
}

