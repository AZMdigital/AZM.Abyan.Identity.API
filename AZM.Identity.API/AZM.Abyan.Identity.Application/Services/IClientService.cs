using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public interface IClientService
{
    Task<List<ClientResponse>> GetClientsAsync(CancellationToken cancellationToken = default);
    Task<ClientResponse?> GetClientByIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
    Task UpdateClientAsync(string clientId, UpdateClientRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientAsync(string clientId, CancellationToken cancellationToken = default);
    Task CreateClientRoleAsync(string clientId, CreateClientRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientRoleAsync(string clientId, string roleName, CancellationToken cancellationToken = default);
}

