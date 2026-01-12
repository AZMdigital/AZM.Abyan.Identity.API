using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class RealmResolverService : IRealmResolverService
{
    private readonly IRepository<Tenant, Guid> _tenantRepository;

    public RealmResolverService(IRepository<Tenant, Guid> tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Guid?> ResolveRealmIdAsync(string realmName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(realmName))
        {
            return null;
        }

        var tenant = await _tenantRepository
            .GetWhere(t => t.Name == realmName && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return tenant?.Id;
    }
}

