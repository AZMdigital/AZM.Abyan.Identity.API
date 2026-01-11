using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class TenantSyncService : ITenantSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IdentityDbContext _dbContext;

    public TenantSyncService(
        IKeycloakService keycloakService,
        IRepository<Tenant, Guid> tenantRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _tenantRepository = tenantRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncTenantsAsync(string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all realms from Keycloak
            var keycloakRealms = await _keycloakService.GetAllRealmsAsync(adminToken, cancellationToken);

            // Get all tenants from local database
            var localTenants = await _tenantRepository.GetWhere().ToListAsync(cancellationToken);

            // Create a dictionary of Keycloak realms by realm name
            var keycloakRealmsDict = keycloakRealms.ToDictionary(r => r.Realm, r => r);

            // Process each Keycloak realm
            foreach (var keycloakRealm in keycloakRealms)
            {
                var localTenant = localTenants.FirstOrDefault(t => t.KeycloakRealmId?.ToString() == keycloakRealm.Id || t.Name == keycloakRealm.Realm);

                if (localTenant == null)
                {
                    // Create new tenant
                    localTenant = new Tenant
                    {
                        Id = Guid.NewGuid(),
                        Name = keycloakRealm.Realm,
                        IsActive = keycloakRealm.Enabled,
                        KeycloakRealmId = Guid.TryParse(keycloakRealm.Id, out var realmId) ? realmId : null,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _tenantRepository.CreateAsync(localTenant, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing tenant
                    localTenant.Name = keycloakRealm.Realm;
                    localTenant.IsActive = keycloakRealm.Enabled;
                    if (Guid.TryParse(keycloakRealm.Id, out var realmId))
                    {
                        localTenant.KeycloakRealmId = realmId;
                    }
                    localTenant.UpdatedAt = DateTime.UtcNow;
                    localTenant.UpdatedBy = Guid.Empty;
                    _tenantRepository.Update(localTenant);
                    result.Updated++;
                }
            }

            // Delete tenants that don't exist in Keycloak
            var keycloakRealmNames = keycloakRealms.Select(r => r.Realm).ToHashSet();
            var tenantsToDelete = localTenants
                .Where(t => !keycloakRealmNames.Contains(t.Name) && t.KeycloakRealmId.HasValue)
                .ToList();

            foreach (var tenantToDelete in tenantsToDelete)
            {
                _dbContext.Tenants.Remove(tenantToDelete);
                result.Deleted++;
            }

            await _tenantRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing tenants: {ex.Message}");
        }

        return result;
    }
}

