namespace AZM.Abyan.Identity.Application.Services;

public interface IPolicySyncService
{
    Task<SyncEntityResult> SyncPoliciesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default);
}

