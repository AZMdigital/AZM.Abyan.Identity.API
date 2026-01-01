using AZM.Abyan.Identity.Application.DTOs.Clients;

namespace AZM.Abyan.Identity.Application.Services;

public interface IClientService
{
    Task<List<ClientResponse>> GetClientsAsync(CancellationToken cancellationToken = default);
    Task<ClientResponse?> GetClientByIdAsync(string clientId, CancellationToken cancellationToken = default);
}

