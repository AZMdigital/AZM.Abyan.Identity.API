using AZM.Identity.Application.DTOs.Clients;

namespace AZM.Identity.Application.Services;

public interface IClientService
{
    Task<List<ClientResponse>> GetClientsAsync(CancellationToken cancellationToken = default);
    Task<ClientResponse?> GetClientByIdAsync(string clientId, CancellationToken cancellationToken = default);
}

