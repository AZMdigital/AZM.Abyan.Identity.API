using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public class ClientService(IKeycloakService keycloakService) : IClientService
{
    private readonly IKeycloakService _keycloakService = keycloakService;

    public async Task<List<ClientResponse>> GetClientsAsync(string realm, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetClientsAsync(realm, adminToken, cancellationToken);
    }

    public async Task<ClientResponse?> GetClientByIdAsync(string realm, string clientId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetClientByIdAsync(realm, clientId, adminToken, cancellationToken);
    }

    public async Task<Guid> CreateClientAsync(string realm, CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var result = await _keycloakService.CreateClientAsync(realm, request, adminToken, cancellationToken);
        return result;
    }

    public async Task UpdateClientAsync(string realm, string clientId, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.UpdateClientAsync(realm, clientId, request, adminToken, cancellationToken);
    }

    public async Task DeleteClientAsync(string realm, string clientId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteClientAsync(realm, clientId, adminToken, cancellationToken);
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
}

