using System.Text.Json;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public interface IClientService
{
    Task<List<ClientResponse>> GetClientsAsync(string realm, CancellationToken cancellationToken = default);
    Task<JsonElement?> GetClientByIdAsync(string realm, string clientId, CancellationToken cancellationToken = default);
    Task<Guid> CreateClientAsync(string realm, CreateClientRequest request, CancellationToken cancellationToken = default);
    Task UpdateClientAsync(string realm, string clientId, UpdateClientRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientAsync(string realm, string clientId, CancellationToken cancellationToken = default);
    Task CreateClientRoleAsync(string realm, string clientId, CreateClientRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientRoleAsync(string realm, string clientId, string roleName, CancellationToken cancellationToken = default);
}

