using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Persistence.Repositories;

public class LicenseRepository(IdentityDbContext db) : ILicenseRepository
{
    public Task<License?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Licenses
          .Include(l => l.Tenant)
          .Include(l => l.LicenseClients)
          .ThenInclude(lc => lc.Client)
          .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task AddAsync(License license, CancellationToken ct = default)
    {
        license.CreatedAt = DateTime.UtcNow;
        await db.Licenses.AddAsync(license, ct);
    }

    public void Update(License license)
    {
        license.UpdatedAt = DateTime.UtcNow;
        db.Licenses.Update(license);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
