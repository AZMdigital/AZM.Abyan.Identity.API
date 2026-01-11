namespace AZM.Abyan.Identity.Application.Services;

public interface IClientSyncService
{
    Task<SyncEntityResult> SyncClientsAsync(string realm, Guid tenantId, string adminToken, CancellationToken cancellationToken = default);
}

