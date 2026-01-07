using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

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

    public async Task CreateClientRoleAsync(string clientId, CreateClientRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.CreateClientRoleAsync(clientId, request, adminToken, cancellationToken);
    }

    public async Task DeleteClientRoleAsync(string clientId, string roleName, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteClientRoleAsync(clientId, roleName, adminToken, cancellationToken);
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

