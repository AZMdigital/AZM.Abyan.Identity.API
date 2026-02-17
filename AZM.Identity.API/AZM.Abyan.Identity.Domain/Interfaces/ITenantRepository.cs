using AZM.Abyan.Identity.Domain.Entities;

namespace AZM.Abyan.Identity.Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tenant entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Tenant entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Tenant>> GetActiveTenants(CancellationToken cancellationToken = default);
}
