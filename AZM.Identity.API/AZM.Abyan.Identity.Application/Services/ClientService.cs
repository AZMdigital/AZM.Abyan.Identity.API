using AZM.Abyan.Identity.Application.DTOs.Clients;

namespace AZM.Abyan.Identity.Application.Services;

public class ClientService : IClientService
{
    private readonly IKeycloakService _keycloakService;

    public ClientService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<List<ClientResponse>> GetClientsAsync(CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetClientsAsync(adminToken, cancellationToken);
    }

    public async Task<ClientResponse?> GetClientByIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetClientByIdAsync(clientId, adminToken, cancellationToken);
    }
}

