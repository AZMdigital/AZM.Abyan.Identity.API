using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _context;

    public TenantRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<List<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.Where(t => !t.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(entity);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await GetByIdAsync(id, cancellationToken);
        if (tenant == null)
            return false;

        tenant.SoftDelete(Guid.Empty);

        _context.Tenants.Update(tenant);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<Tenant?> GetTenantByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted, cancellationToken);
    }

    public async Task<List<Tenant>> GetActiveTenants(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);
    }
}
