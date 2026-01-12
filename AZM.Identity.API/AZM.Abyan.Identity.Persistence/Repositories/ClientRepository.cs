using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Persistence.DbContexts;
using AZM.Abyan.Identity.Persistence.Repositories.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Persistence.Persistence.Repositories;

public class ClientRepository(IdentityDbContext context, ICurrentUserService _currentUserService)
    : Repository<Client, Guid, IdentityDbContext>(context), IClientRepository
{
    public async Task<Guid> AddAsync(Client metadata, CancellationToken cancellationToken = default)
    {
        await CreateAsync(metadata, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return metadata.Id;
    }
    public async Task<bool> DeleteClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await GetWhere(s=>s.KeycloakClientId==id).FirstOrDefaultAsync(cancellationToken);

        if (client == null) return false;

        client.SoftDelete();
        Update(client);
        await SaveChangesAsync(cancellationToken);
        return true;
    }
    public async Task<Client?> GetClientByKeycloakIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetWhere(f => f.KeycloakClientId == id)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        Update(client);
        await SaveChangesAsync(cancellationToken);
        return true;
    }
    //public async Task<IReadOnlyList<OssFile>> AddRangeAsync(List<OssFile> files, CancellationToken cancellationToken = default)
    //{
    //    await CreateManyAsync(files, cancellationToken);
    //    await SaveChangesAsync(cancellationToken);
    //    return files;
    //}

    //public new async Task<OssFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    //    => await base.GetByIdAsync(id, cancellationToken);

    //public new async Task<List<OssFile>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    //{
    //    if (ids == null || !ids.Any())
    //        return [];

    //    var idList = ids.ToList();

    //    return await GetWhere(f => idList.Contains(f.Id))
    //        .ToListAsync(cancellationToken);
    //}

    //public async Task<OssFile?> GetByObjectKeyAsync(string objectKey, CancellationToken cancellationToken = default)
    //    => await GetWhere(f => f.ObjectKey == objectKey)
    //        .FirstOrDefaultAsync(cancellationToken);

    //public async Task<List<OssFile>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    //    => await GetWhere(f => f.CreatedBy == userId) 
    //        .OrderByDescending(f => f.CreatedAt)
    //        .ToListAsync(cancellationToken);

    //public async Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken = default)
    //    => await GetWhere(f => f.ObjectKey == objectKey)
    //        .AnyAsync(cancellationToken);

    //public async Task<List<OssFile>> GetAllAsync(CancellationToken cancellationToken = default)
    //    => await GetWhere()
    //        .AsNoTracking()
    //        .OrderByDescending(f => f.CreatedAt)
    //        .ToListAsync(cancellationToken);

    //public async Task UpdateAsync(OssFile metadata, CancellationToken cancellationToken = default)
    //{
    //    Update(metadata);
    //    await SaveChangesAsync(cancellationToken);
    //}

    //public async Task DeleteAsync(OssFile metadata, CancellationToken cancellationToken = default)
    //{
    //    var userId =_currentUserService.GetCurrentUserId() ?? Guid.Empty;
    //    metadata.SoftDelete(userId);
    //    Update(metadata);
    //    await SaveChangesAsync(cancellationToken);
    //}

    //public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    //{
    //    if (ids == null || !ids.Any())
    //        return;

    //    var entities = await GetByIdsAsync(ids, cancellationToken);
    //    if (!entities.Any())
    //        return;
    //    // Get current user ID if available
    //    var userId =  _currentUserService.GetCurrentUserId() ?? Guid.Empty;

    //    foreach (var entity in entities)
    //    {
    //        entity.SoftDelete(userId);
    //    }

    //    UpdateMulti(entities);
    //    await SaveChangesAsync(cancellationToken);
    //}
}
