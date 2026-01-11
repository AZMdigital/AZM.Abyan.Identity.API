namespace AZM.Abyan.Identity.Application.Services;

public interface IResourceSyncService
{
    Task<SyncEntityResult> SyncResourcesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default);
}

