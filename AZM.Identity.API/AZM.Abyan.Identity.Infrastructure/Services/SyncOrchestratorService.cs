using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class SyncOrchestratorService : ISyncOrchestratorService
{
    private readonly IKeycloakService _keycloakService;
    private readonly ITenantSyncService _tenantSyncService;
    private readonly IUserSyncService _userSyncService;
    private readonly IClientSyncService _clientSyncService;
    private readonly IRoleSyncService _roleSyncService;
    private readonly IPermissionKeycloakSyncService _permissionSyncService;
    private readonly ITenantUserRoleSyncService _tenantUserRoleSyncService;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<Client, Guid> _clientRepository;
    private readonly KeycloakConfigurations _keycloakConfigurations;

    public SyncOrchestratorService(
        IKeycloakService keycloakService,
        ITenantSyncService tenantSyncService,
        IUserSyncService userSyncService,
        IClientSyncService clientSyncService,
        IRoleSyncService roleSyncService,
        IPermissionKeycloakSyncService permissionSyncService,
        ITenantUserRoleSyncService tenantUserRoleSyncService,
        IRepository<Tenant, Guid> tenantRepository,
        IRepository<Client, Guid> clientRepository,
        IOptions<KeycloakConfigurations> keycloakConfigurations)
    {
        _keycloakService = keycloakService;
        _tenantSyncService = tenantSyncService;
        _userSyncService = userSyncService;
        _clientSyncService = clientSyncService;
        _roleSyncService = roleSyncService;
        _permissionSyncService = permissionSyncService;
        _tenantUserRoleSyncService = tenantUserRoleSyncService;
        _tenantRepository = tenantRepository;
        _clientRepository = clientRepository;
        _keycloakConfigurations = keycloakConfigurations.Value;
    }

    public async Task<SyncResult> SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var result = new SyncResult();

        // Start transaction
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _tenantRepository.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get admin token
                var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

                // Step 1: Sync Tenants (Realms) - must be first
                result.EntityResults["Tenants"] = await _tenantSyncService.SyncTenantsAsync(adminToken, cancellationToken);
                if (result.EntityResults["Tenants"].Errors.Any())
                {
                    result.Errors.AddRange(result.EntityResults["Tenants"].Errors);
                }

                // Get configured tenants and clients from appsettings.json
                var configuredTenants = _keycloakConfigurations.Tenants.Keys.ToList();
                var configuredClients = _keycloakConfigurations.Tenants
                    .SelectMany(t => t.Value.KeycloakFormbuilder != null 
                        ? new[] { new { TenantName = t.Key, ClientId = t.Value.KeycloakFormbuilder.ClientId } }
                        : Enumerable.Empty<dynamic>())
                    .ToList();

                // Get all tenants to process, but filter by configured tenants
                var tenants = await _tenantRepository.GetWhere(t => configuredTenants.Contains(t.Name)).ToListAsync(cancellationToken);

                // Process each configured tenant/realm
                foreach (var tenant in tenants)
                {
                    var realm = tenant.Name;

                    // Step 2: Sync Clients for this realm
                    result.EntityResults[$"Clients-{realm}"] = await _clientSyncService.SyncClientsAsync(realm, tenant.Id, adminToken, cancellationToken);
                    if (result.EntityResults[$"Clients-{realm}"].Errors.Any())
                    {
                        result.Errors.AddRange(result.EntityResults[$"Clients-{realm}"].Errors);
                    }

                    // Step 3: Sync Users for this realm
                    result.EntityResults[$"Users-{realm}"] = await _userSyncService.SyncUsersAsync(realm, tenant.Id, adminToken, cancellationToken);
                    if (result.EntityResults[$"Users-{realm}"].Errors.Any())
                    {
                        result.Errors.AddRange(result.EntityResults[$"Users-{realm}"].Errors);
                    }

                    // Get configured client IDs for this tenant
                    var tenantConfig = _keycloakConfigurations.Tenants.GetValueOrDefault(realm);
                    var allowedClientIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (tenantConfig != null)
                    {
                        if (!string.IsNullOrEmpty(tenantConfig.KeycloakFormbuilder?.ClientId))
                            allowedClientIds.Add(tenantConfig.KeycloakFormbuilder.ClientId);
                        if (!string.IsNullOrEmpty(tenantConfig.KeycloakWorkflow?.ClientId))
                            allowedClientIds.Add(tenantConfig.KeycloakWorkflow.ClientId);
                    }

                    // Get all clients for this tenant
                    var allClients = await _clientRepository
                        .GetWhere(c => c.RealmId == tenant.Id)
                        .ToListAsync(cancellationToken);

                    // Filter clients by configured client IDs (match by Name which should be the ClientId)
                    var clients = allClients
                        .Where(c => allowedClientIds.Contains(c.Name))
                        .ToList();

                    // Process each configured client
                    foreach (var client in clients)
                    {
                        var clientId = client.Id.ToString();

                        // Step 4: Sync Roles for this client (pass both Keycloak client ID and local client ID)
                        result.EntityResults[$"Roles-{realm}-{client.Name}"] = await _roleSyncService.SyncRolesAsync(realm, clientId, client.Id, adminToken, cancellationToken);
                        if (result.EntityResults[$"Roles-{realm}-{client.Name}"].Errors.Any())
                        {
                            result.Errors.AddRange(result.EntityResults[$"Roles-{realm}-{client.Name}"].Errors);
                        }

                        // Step 5: Sync Permissions (roles with Controller/Action attributes) for this client
                        result.EntityResults[$"Permissions-{realm}-{client.Name}"] = await _permissionSyncService.SyncPermissionsAsync(realm, clientId, adminToken, cancellationToken);
                        if (result.EntityResults[$"Permissions-{realm}-{client.Name}"].Errors.Any())
                        {
                            result.Errors.AddRange(result.EntityResults[$"Permissions-{realm}-{client.Name}"].Errors);
                        }
                    }

                    // Step 6: Sync TenantUserRoles for this realm (must be last)
                    result.EntityResults[$"TenantUserRoles-{realm}"] = await _tenantUserRoleSyncService.SyncTenantUserRolesAsync(realm, adminToken, cancellationToken);
                    if (result.EntityResults[$"TenantUserRoles-{realm}"].Errors.Any())
                    {
                        result.Errors.AddRange(result.EntityResults[$"TenantUserRoles-{realm}"].Errors);
                    }
                }

                result.Success = !result.Errors.Any();

                if (result.Success)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
                else
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                result.Success = false;
                result.Errors.Add($"Critical error during sync: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            result.Success = false;
            result.Errors.Add($"Critical error starting transaction: {ex.Message}");
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        return result;
    }
}

