namespace AZM.Abyan.Identity.Application.Services;

public interface IScopeSyncService
{
    Task<SyncEntityResult> SyncScopesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default);
}

