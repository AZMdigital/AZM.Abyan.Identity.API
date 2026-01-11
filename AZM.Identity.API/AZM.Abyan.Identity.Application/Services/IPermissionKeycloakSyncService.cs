namespace AZM.Abyan.Identity.Application.Services;

public interface IPermissionKeycloakSyncService
{
    Task<SyncEntityResult> SyncPermissionsAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default);
}

