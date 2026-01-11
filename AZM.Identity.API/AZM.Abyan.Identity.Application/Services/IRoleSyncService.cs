namespace AZM.Abyan.Identity.Application.Services;

public interface IRoleSyncService
{
    Task<SyncEntityResult> SyncRolesAsync(string realm, string keycloakClientId, Guid localClientId, string adminToken, CancellationToken cancellationToken = default);
}

