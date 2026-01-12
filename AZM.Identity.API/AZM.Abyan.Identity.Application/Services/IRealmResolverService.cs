namespace AZM.Abyan.Identity.Application.Services;

public interface IRealmResolverService
{
    /// <summary>
    /// Resolves realm ID (Tenant.Id) from realm name by looking up Tenant in database
    /// </summary>
    /// <param name="realmName">The realm/tenant name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The Tenant.Id (Keycloak realm ID) if found, null otherwise</returns>
    Task<Guid?> ResolveRealmIdAsync(string realmName, CancellationToken cancellationToken = default);
}

