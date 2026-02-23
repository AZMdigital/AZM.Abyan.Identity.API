using AZM.Abyan.Identity.Domain.Entities;

namespace AZM.Abyan.Identity.Domain.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetClientByKeycloakIdAsync(Guid id, CancellationToken cancellationToken = default);
    //Task<List<OssFile>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    //Task<List<OssFile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Guid> AddAsync(Client client, CancellationToken cancellationToken = default);
    Task<bool> DeleteClientAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default);
    Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}