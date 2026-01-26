using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class TenantSyncService : ITenantSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IdentityDbContext _dbContext;
    private readonly KeycloakConfigurations _keycloakConfigurations;

    public TenantSyncService(
        IKeycloakService keycloakService,
        IRepository<Tenant, Guid> tenantRepository,
        IdentityDbContext dbContext,
        IOptions<KeycloakConfigurations> keycloakConfigurations)
    {
        _keycloakService = keycloakService;
        _tenantRepository = tenantRepository;
        _dbContext = dbContext;
        _keycloakConfigurations = keycloakConfigurations.Value;
    }

    public async Task<SyncEntityResult> SyncTenantsAsync(string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all realms from Keycloak
            var allKeycloakRealms = await _keycloakService.GetAllRealmsAsync(adminToken, cancellationToken);

            // Filter to only configured tenants (skip "master" and other non-configured realms)
            var configuredTenantNames = _keycloakConfigurations.Tenants.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var keycloakRealms = allKeycloakRealms
                .Where(r => configuredTenantNames.Contains(r.Realm))
                .ToList();

            // Get all tenants from local database
            var localTenants = await _tenantRepository.GetWhere().ToListAsync(cancellationToken);

            // Create a dictionary of Keycloak realms by realm name
            var keycloakRealmsDict = keycloakRealms.ToDictionary(r => r.Realm, r => r);

            // Process each configured Keycloak realm
            foreach (var keycloakRealm in keycloakRealms)
            {
                if (!Guid.TryParse(keycloakRealm.Id, out var keycloakRealmId))
                {
                    result.Errors.Add($"Invalid Keycloak realm ID format: {keycloakRealm.Id}");
                    continue;
                }

                var localTenant = localTenants.FirstOrDefault(t => t.Id == keycloakRealmId || t.Name == keycloakRealm.Realm);

                if (localTenant == null)
                {
                    // Create new tenant
                    localTenant = new Tenant
                    {
                        Id = keycloakRealmId,
                        Name = keycloakRealm.Realm,
                        IsActive = keycloakRealm.Enabled,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _tenantRepository.CreateAsync(localTenant, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing tenant - ensure ID matches Keycloak
                    if (localTenant.Id != keycloakRealmId)
                    {
                        localTenant.Id = keycloakRealmId;
                    }
                    localTenant.Name = keycloakRealm.Realm;
                    localTenant.IsActive = keycloakRealm.Enabled;
                    localTenant.UpdatedAt = DateTime.UtcNow;
                    localTenant.UpdatedBy = Guid.Empty;
                    _tenantRepository.Update(localTenant);
                    result.Updated++;
                }
            }

            // Delete tenants that don't exist in Keycloak (only for configured tenants)
            var keycloakRealmIds = keycloakRealms
                .Where(r => Guid.TryParse(r.Id, out _))
                .Select(r => Guid.Parse(r.Id))
                .ToHashSet();
            var tenantsToDelete = localTenants
                .Where(t => configuredTenantNames.Contains(t.Name) && !keycloakRealmIds.Contains(t.Id))
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

