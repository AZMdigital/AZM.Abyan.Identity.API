using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class SyncOrchestratorService(
    IKeycloakService keycloakService,
    ITenantSyncService tenantSyncService,
    IUserSyncService userSyncService,
    IClientSyncService clientSyncService,
    IRoleSyncService roleSyncService,
    IScopeSyncService scopeSyncService,
    IResourceSyncService resourceSyncService,
    IPolicySyncService policySyncService,
    IPermissionKeycloakSyncService permissionSyncService,
    ITenantUserRoleSyncService tenantUserRoleSyncService,
    IRepository<Tenant, Guid> tenantRepository,
    IRepository<Client, Guid> clientRepository) : ISyncOrchestratorService
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly ITenantSyncService _tenantSyncService = tenantSyncService;
    private readonly IUserSyncService _userSyncService = userSyncService;
    private readonly IClientSyncService _clientSyncService = clientSyncService;
    private readonly IRoleSyncService _roleSyncService = roleSyncService;
    private readonly IScopeSyncService _scopeSyncService = scopeSyncService;
    private readonly IResourceSyncService _resourceSyncService = resourceSyncService;
    private readonly IPolicySyncService _policySyncService = policySyncService;
    private readonly IPermissionKeycloakSyncService _permissionSyncService = permissionSyncService;
    private readonly ITenantUserRoleSyncService _tenantUserRoleSyncService = tenantUserRoleSyncService;
    private readonly IRepository<Tenant, Guid> _tenantRepository = tenantRepository;
    private readonly IRepository<Client, Guid> _clientRepository = clientRepository;

    public async Task<SyncResult> SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var result = new SyncResult();

        // Start transaction
        //IDbContextTransaction? transaction = null;
        try
        {
            //transaction = await _tenantRepository.BeginTransactionAsync(cancellationToken);

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

                // Get all tenants to process
                var tenants = await _tenantRepository.GetWhere().ToListAsync(cancellationToken);

                // Process each tenant/realm (all tenants now have IDs from Keycloak)
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

                    // Get all clients for this tenant
                    var clients = await _clientRepository.GetWhere(c => c.RealmId == tenant.Id).ToListAsync(cancellationToken);

                    // Process each client (all clients now have IDs from Keycloak)
                    foreach (var client in clients)
                    {
                        var clientId = client.Id.ToString();

                        // Step 4: Sync Roles for this client (pass both Keycloak client ID and local client ID)
                        result.EntityResults[$"Roles-{realm}-{client.Name}"] = await _roleSyncService.SyncRolesAsync(realm, clientId, client.Id, adminToken, cancellationToken);
                        if (result.EntityResults[$"Roles-{realm}-{client.Name}"].Errors.Any())
                        {
                            result.Errors.AddRange(result.EntityResults[$"Roles-{realm}-{client.Name}"].Errors);
                        }

                        // Step 5: Sync Scopes for this client
                        result.EntityResults[$"Scopes-{realm}-{client.Name}"] = await _scopeSyncService.SyncScopesAsync(realm, clientId, adminToken, cancellationToken);
                        if (result.EntityResults[$"Scopes-{realm}-{client.Name}"].Errors.Any())
                        {
                            result.Errors.AddRange(result.EntityResults[$"Scopes-{realm}-{client.Name}"].Errors);
                        }

                        // Step 6: Sync Resources for this client
                        result.EntityResults[$"Resources-{realm}-{client.Name}"] = await _resourceSyncService.SyncResourcesAsync(realm, clientId, adminToken, cancellationToken);
                        if (result.EntityResults[$"Resources-{realm}-{client.Name}"].Errors.Any())
                        {
                            result.Errors.AddRange(result.EntityResults[$"Resources-{realm}-{client.Name}"].Errors);
                        }

                        // Step 7: Sync Policies for this client
                        result.EntityResults[$"Policies-{realm}-{client.Name}"] = await _policySyncService.SyncPoliciesAsync(realm, clientId, adminToken, cancellationToken);
                        if (result.EntityResults[$"Policies-{realm}-{client.Name}"].Errors.Any())
                        {
                            result.Errors.AddRange(result.EntityResults[$"Policies-{realm}-{client.Name}"].Errors);
                        }

                        // Step 8: Sync Permissions for this client
                        result.EntityResults[$"Permissions-{realm}-{client.Name}"] = await _permissionSyncService.SyncPermissionsAsync(realm, clientId, adminToken, cancellationToken);
                        if (result.EntityResults[$"Permissions-{realm}-{client.Name}"].Errors.Any())
                        {
                            result.Errors.AddRange(result.EntityResults[$"Permissions-{realm}-{client.Name}"].Errors);
                        }
                    }

                    // Step 9: Sync TenantUserRoles for this realm (must be last)
                    result.EntityResults[$"TenantUserRoles-{realm}"] = await _tenantUserRoleSyncService.SyncTenantUserRolesAsync(realm, adminToken, cancellationToken);
                    if (result.EntityResults[$"TenantUserRoles-{realm}"].Errors.Any())
                    {
                        result.Errors.AddRange(result.EntityResults[$"TenantUserRoles-{realm}"].Errors);
                    }
                }

                result.Success = !result.Errors.Any();

                //if (result.Success)
                //{
                //    await transaction.CommitAsync(cancellationToken);
                //}
                //else
                //{
                //    await transaction.RollbackAsync(cancellationToken);
                //}
            }
            catch (Exception ex)
            {
                //if (transaction != null)
                //{
                //    await transaction.RollbackAsync(cancellationToken);
                //}
                result.Success = false;
                result.Errors.Add($"Critical error during sync: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            //if (transaction != null)
            //{
            //    await transaction.RollbackAsync(cancellationToken);
            //}
            result.Success = false;
            result.Errors.Add($"Critical error starting transaction: {ex.Message}");
        }
        finally
        {
            //if (transaction != null)
            //{
            //    await transaction.DisposeAsync();
            //}
        }

        return result;
    }
}

