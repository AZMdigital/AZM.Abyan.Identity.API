using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Users;

namespace AZM.Abyan.Identity.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IKeycloakService _keycloakService;

    public OrganizationService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<List<OrganizationResponse>> GetOrganizationsAsync(string realm, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetOrganizationsAsync(realm, adminToken, null, cancellationToken);
    }

    public async Task<OrganizationResponse?> GetOrganizationByIdAsync(string realm, string organizationId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetOrganizationByIdAsync(realm, organizationId, adminToken, cancellationToken);
    }

    public async Task<string> CreateOrganizationAsync(string realm, CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.CreateOrganizationAsync(realm, request, adminToken, cancellationToken);
    }

    public async Task UpdateOrganizationAsync(string realm, string organizationId, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.UpdateOrganizationAsync(realm, organizationId, request, adminToken, cancellationToken);
    }

    public async Task DeleteOrganizationAsync(string realm, string organizationId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteOrganizationAsync(realm, organizationId, adminToken, cancellationToken);
    }

    public async Task<List<UserResponse>> GetOrganizationMembersAsync(string realm, string organizationId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetOrganizationMembersAsync(realm, organizationId, adminToken, cancellationToken);
    }

    public async Task AddMemberToOrganizationAsync(string realm, string organizationId, string userId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.AddMemberToOrganizationAsync(realm, organizationId, userId, adminToken, cancellationToken);
    }

    public async Task RemoveMemberFromOrganizationAsync(string realm, string organizationId, string memberId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.RemoveMemberFromOrganizationAsync(realm, organizationId, memberId, adminToken, cancellationToken);
    }
}
