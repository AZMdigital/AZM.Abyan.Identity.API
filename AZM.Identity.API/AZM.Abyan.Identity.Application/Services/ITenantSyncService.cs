namespace AZM.Abyan.Identity.Application.Services;

public interface ITenantSyncService
{
    Task<SyncEntityResult> SyncTenantsAsync(string adminToken, CancellationToken cancellationToken = default);
}

