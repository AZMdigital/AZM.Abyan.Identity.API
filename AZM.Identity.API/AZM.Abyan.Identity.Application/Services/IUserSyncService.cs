namespace AZM.Abyan.Identity.Application.Services;

public interface IUserSyncService
{
    Task<SyncEntityResult> SyncUsersAsync(string realm, Guid tenantId, string adminToken, CancellationToken cancellationToken = default);
}

