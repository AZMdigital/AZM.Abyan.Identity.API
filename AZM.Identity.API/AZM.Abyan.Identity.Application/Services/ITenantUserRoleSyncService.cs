namespace AZM.Abyan.Identity.Application.Services;

public interface ITenantUserRoleSyncService
{
    Task<SyncEntityResult> SyncTenantUserRolesAsync(string realm, string adminToken, CancellationToken cancellationToken = default);
}

