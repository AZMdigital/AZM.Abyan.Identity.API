using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public class RealmAdminService : IRealmAdminService
{
    private readonly IKeycloakService _keycloakService;

    public RealmAdminService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<List<RealmResponse>> GetAllRealmsAsync(CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetAllRealmsAsync(adminToken, cancellationToken);
    }

    public async Task<RealmResponse?> GetRealmByNameAsync(string realmName, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetRealmByNameAsync(realmName, adminToken, cancellationToken);
    }

    public async Task CreateRealmAsync(CreateRealmRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.CreateRealmAsync(request, adminToken, cancellationToken);
    }

    public async Task UpdateRealmAsync(string realmName, UpdateRealmRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.UpdateRealmAsync(realmName, request, adminToken, cancellationToken);
    }

    public async Task UpdateRealmPasswordPolicyAsync(string realmName, UpdateRealmPasswordPolicyRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.UpdateRealmPasswordPolicyAsync(realmName, request, adminToken, cancellationToken);
    }

    public async Task DeleteRealmAsync(string realmName, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteRealmAsync(realmName, adminToken, cancellationToken);
    }

    public async Task<List<RealmRoleResponse>> GetRealmRolesAsync(string realm, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetRealmRolesAsync(realm, adminToken, cancellationToken);
    }

    public async Task CreateRealmRoleAsync(CreateRealmRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.CreateRealmRoleAsync(request, adminToken, cancellationToken);
    }

    public async Task UpdateRealmRoleAsync(string realm, string roleName, UpdateRealmRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.UpdateRealmRoleAsync(realm, roleName, request, adminToken, cancellationToken);
    }

    public async Task DeleteRealmRoleAsync(string realm, string roleName, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteRealmRoleAsync(realm, roleName, adminToken, cancellationToken);
    }

    public async Task AssignRealmRoleToUserAsync(AssignRealmRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.AssignRealmRoleToUserAsync(request, adminToken, cancellationToken);
    }

    public async Task RemoveRealmRoleFromUserAsync(AssignRealmRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.RemoveRealmRoleFromUserAsync(request, adminToken, cancellationToken);
    }
}
