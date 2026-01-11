using System.Text.Json;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

    public async Task<JsonElement?> GetClientByIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetClientByIdAsync(clientId, adminToken, cancellationToken);
    }

    public async Task<Guid> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var result= await _keycloakService.CreateClientAsync(request, adminToken, cancellationToken);

        return result;
    }

    public async Task UpdateClientAsync(string clientId, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.UpdateClientAsync(clientId, request, adminToken, cancellationToken);
    }

    public async Task DeleteClientAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteClientAsync(clientId, adminToken, cancellationToken);
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
}

