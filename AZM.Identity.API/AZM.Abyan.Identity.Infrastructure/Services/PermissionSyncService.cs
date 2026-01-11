using System.Reflection;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Infrastructure.Security;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class PermissionSyncService : IPermissionSyncService
{
    private readonly IdentityDbContext _dbContext;
    private readonly IKeycloakService _keycloakService;
    private readonly KeycloakConfiguration _keycloakConfig;
    private readonly ILogger<PermissionSyncService> _logger;

    public PermissionSyncService(
        IdentityDbContext dbContext,
        IKeycloakService keycloakService,
        IOptions<KeycloakConfiguration> keycloakConfig,
        ILogger<PermissionSyncService> logger)
    {
        _dbContext = dbContext;
        _keycloakService = keycloakService;
        _keycloakConfig = keycloakConfig.Value;
        _logger = logger;
    }

    public async Task SyncPermissionsAsync(Assembly assembly, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting permission sync...");

        // 1. Discover Permissions
        var discoveredPermissions = PermissionDiscovery.Discover(assembly);
        _logger.LogInformation($"Discovered {discoveredPermissions.Count} unique permission strings.");
        foreach(var p in discoveredPermissions)
        {
             _logger.LogDebug($"Discovered: {p.Name} (Resource: {p.Resources.Name}, Action: {p.Scope.Name})");
        }

        // 2. Sync with Database
        await SyncWithDatabaseAsync(discoveredPermissions, cancellationToken);
        _logger.LogInformation("Database sync completed.");

        // 3. Sync with Keycloak
        await SyncWithKeycloakAsync(discoveredPermissions, cancellationToken);
        
        _logger.LogInformation("Permission sync process finished.");
    }

    private async Task SyncWithDatabaseAsync(List<Permission> permissions, CancellationToken cancellationToken)
    {
        var existingPermissions = await _dbContext.Permissions.ToListAsync(cancellationToken);
        
        var newPermissions = permissions
            .Where(p => !existingPermissions.Any(ep => ep.Name == p.Name))
            .ToList();

        if (newPermissions.Any())
        {
            _logger.LogInformation($"Adding {newPermissions.Count} new permissions to database.");
            await _dbContext.Permissions.AddRangeAsync(newPermissions, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("No new permissions to add to database.");
        }
    }

    private async Task SyncWithKeycloakAsync(List<Permission> permissions, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Get Client UUID
            var realm = _keycloakConfig.Realm;
            var clients = await _keycloakService.GetClientsAsync(realm, adminToken, cancellationToken);
            var targetClient = clients.FirstOrDefault(c => c.ClientId == _keycloakConfig.ClientId);

            if (targetClient == null)
            {
                _logger.LogError($"Client with ClientId '{_keycloakConfig.ClientId}' not found in Keycloak realm '{realm}'.");
                return;
            }

            var clientUuid = targetClient.Id;
            _logger.LogInformation($"Syncing with Keycloak Client: {_keycloakConfig.ClientId} (UUID: {clientUuid}) in realm '{realm}'");

            // Ensure Authorization Services are enabled
            if (!targetClient.AuthorizationServicesEnabled)
            {
                _logger.LogInformation($"Authorization Services not enabled for client. Enabling now...");
                await _keycloakService.UpdateClientAsync(realm, clientUuid, new UpdateClientRequest
                {
                    ClientId = targetClient.ClientId,
                    Name = targetClient.Name,
                    Description = targetClient.Description,
                    Enabled = targetClient.Enabled,
                    Protocol = targetClient.Protocol,
                    PublicClient = false, // Must be confidential for AuthZ/ServiceAccounts
                    BearerOnly = false,
                    ServiceAccountsEnabled = true, // Required for AuthZ often
                    AuthorizationServicesEnabled = true,
                    RedirectUris = targetClient.RedirectUris,
                    WebOrigins = targetClient.WebOrigins
                }, adminToken, cancellationToken);
                _logger.LogInformation("Authorization Services successfully enabled.");
            }

            // Get existing client roles ONCE
            var existingRoles = await _keycloakService.GetClientRolesAsync(realm, clientUuid, adminToken, cancellationToken);
            var existingRoleNames = existingRoles.Select(r => r.Name).ToHashSet();

            // Group by Controller (Resource)
            var resourceGroups = permissions.GroupBy(p => p.Resources.Name).ToList();
            _logger.LogInformation($"Grouped into {resourceGroups.Count} resources (controllers).");

            foreach (var group in resourceGroups)
            {
                var controllerName = group.Key;
                var actionPermissions = group.ToList();
                var scopeNames = actionPermissions.Select(p => p.Resources.Name).ToList();

                _logger.LogInformation($"Processing Resource: {controllerName} with {actionPermissions.Count} actions.");

                // 1. Manage Keycloak Resource
                var resourceName = $"res:{controllerName}";
                var existingResource = await _keycloakService.GetResourceAsync(realm, clientUuid, resourceName, adminToken, cancellationToken);
                
                Guid keycloakResourceId;
                var resourceDto = new AZM.Abyan.Identity.Application.DTOs.AuthZ.ResourceDto
                {
                    Name = resourceName,
                    DisplayName = $"{controllerName} Controller",
                    Type = "urn:abyan:resource:controller",
                    Uris = new List<string> { $"/{controllerName}" },
                    Scopes = scopeNames.Select(s => new AZM.Abyan.Identity.Application.DTOs.AuthZ.ScopeDto { Name = s }).ToList()
                };

                if (existingResource == null)
                {
                    _logger.LogInformation($"Creating Keycloak Resource '{resourceName}'...");
                    keycloakResourceId = await _keycloakService.CreateResourceAsync(realm, clientUuid, resourceDto, adminToken, cancellationToken);
                    _logger.LogInformation($"Successfully created Resource '{resourceName}' (ID: {keycloakResourceId}).");
                }
                else
                {
                    _logger.LogInformation($"Keycloak Resource '{resourceName}' already exists. Updating...");
                    keycloakResourceId = existingResource.Id.Value;
                    resourceDto.Id = keycloakResourceId;
                    await _keycloakService.UpdateResourceAsync(realm, clientUuid, resourceDto, adminToken, cancellationToken);
                    _logger.LogInformation($"Successfully updated Resource '{resourceName}'.");
                }

                // 2. Manage Roles, Policies and Permissions for each Action
                foreach (var permDef in actionPermissions)
                {
                    _logger.LogDebug($"Processing AuthZ for Action: {permDef.Resources.Name} (Permission: {permDef.Name})");

                    // Get or Add to DB context to track updates
                    var dbPermission = await _dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == permDef.Name, cancellationToken);
                    if (dbPermission == null)
                    {
                        dbPermission = new Permission
                        {
                            Name = permDef.Name,
                            ResourceId = permDef.ResourceId,
                            ScopeId = permDef.ScopeId,
                            Description = permDef.Description
                        };
                        _dbContext.Permissions.Add(dbPermission);
                    }

                    dbPermission.Resources.KeycloakResourceId = keycloakResourceId;

                    // a. Ensure Client Role exists
                    if (!existingRoleNames.Contains(permDef.Name))
                    {
                        _logger.LogInformation($"Creating client role '{permDef.Name}' in Keycloak.");
                        await _keycloakService.CreateClientRoleAsync(realm, clientUuid, new CreateClientRoleRequest
                        {
                            Name = permDef.Name,
                            Description = permDef.Description ?? string.Empty
                        }, adminToken, cancellationToken);
                        
                        existingRoleNames.Add(permDef.Name);
                    }

                    // b. Manage Policy (Role-based)
                    var policyName = $"pol:{controllerName}:{permDef.Scope.Name}";
                    var existingPolicy = await _keycloakService.GetPolicyAsync(realm, clientUuid, policyName, adminToken, cancellationToken);
                    string keycloakPolicyId;
                    if (existingPolicy == null)
                    {
                        _logger.LogInformation($"Creating Keycloak Policy '{policyName}'...");
                        keycloakPolicyId = await _keycloakService.CreateRolePolicyAsync(realm, clientUuid, policyName, new[] { permDef.Name }, adminToken, cancellationToken);
                        _logger.LogInformation($"Successfully created Policy '{policyName}' (ID: {keycloakPolicyId}).");
                    }
                    else
                    {
                        keycloakPolicyId = existingPolicy.Id;
                        _logger.LogDebug($"Policy '{policyName}' already exists.");
                    }

                    // c. Manage Scope-based Permission
                    var authzPermissionName = $"perm:{controllerName}:{permDef.Scope.Name}";
                    var existingAuthzPerm = await _keycloakService.GetPermissionAsync(realm, clientUuid, authzPermissionName, adminToken, cancellationToken);
                    string keycloakAuthzPermId;
                    if (existingAuthzPerm == null)
                    {
                        _logger.LogInformation($"Creating Keycloak Permission '{authzPermissionName}'...");
                        keycloakAuthzPermId = await _keycloakService.CreateScopePermissionAsync(
                            realm,
                            clientUuid, 
                            authzPermissionName, 
                            [resourceName], 
                            [permDef.Scope.Name], 
                            [policyName], 
                            adminToken, 
                            cancellationToken);
                        _logger.LogInformation($"Successfully created Permission '{authzPermissionName}' (ID: {keycloakAuthzPermId}).");
                    }
                    else
                    {
                        keycloakAuthzPermId = existingAuthzPerm.Id;
                        _logger.LogDebug($"Permission '{authzPermissionName}' already exists.");
                    }

                    // Update DB Permission with IDs
                    dbPermission.KeycloakPermissionId =Guid.Parse(keycloakAuthzPermId);                    
                    // Note: Keycloak doesn't return a separate Scope ID in this flow easily, 
                    // usually it's tied to the resource or searched separately. 
                    // For now, we leave KeycloakScopeId if not easily available.
                    
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            _logger.LogInformation("Keycloak optimization/authz sync completed successfully with DB mapping updates.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing permissions with Keycloak.");
            throw; 
        }
    }
}
