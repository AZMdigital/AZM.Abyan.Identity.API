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
        foreach (var p in discoveredPermissions)
        {
            _logger.LogDebug($"Discovered: {p.Name} (Resource: {p.ResourceName}, Action: {p.ScopeName})");
        }

        // 2. Sync with Keycloak and Database
        await SyncWithKeycloakAsync(discoveredPermissions, cancellationToken);

        _logger.LogInformation("Permission sync process finished.");
    }


    private async Task SyncWithKeycloakAsync(List<DiscoveredPermission> permissions, CancellationToken cancellationToken)
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

            Guid clientUuid = targetClient.Id;
            _logger.LogInformation($"Syncing with Keycloak Client: {_keycloakConfig.ClientId} (UUID: {clientUuid}) in realm '{realm}'");

            // Ensure Authorization Services are enabled
            _logger.LogInformation($"Authorization Services check for client...");
            await _keycloakService.UpdateClientAsync(realm, clientUuid.ToString(), new UpdateClientRequest
            {
                ClientId = targetClient.ClientId,
                Name = targetClient.Name,
                Description = targetClient.Description,
                Enabled = true,
                Protocol = "openid-connect",
                PublicClient = false,
                BearerOnly = false,
                ServiceAccountsEnabled = true,
                AuthorizationServicesEnabled = true,
                RedirectUris = Array.Empty<string>().ToList(),
                WebOrigins = Array.Empty<string>().ToList()
            }, adminToken, cancellationToken);

            // Get existing client roles ONCE
            var existingKeycloakRoles = await _keycloakService.GetClientRolesAsync(realm, clientUuid.ToString(), adminToken, cancellationToken);
            var existingRoleNames = existingKeycloakRoles.Select(r => r.Name).ToHashSet();

            // Group by Resource
            var resourceGroups = permissions.GroupBy(p => p.ResourceName).ToList();
            _logger.LogInformation($"Grouped into {resourceGroups.Count} resources.");

            foreach (var group in resourceGroups)
            {
                var resourceName = group.Key;
                var actionPermissions = group.ToList();
                var keycloakResourceName = $"res:{resourceName}";

                _logger.LogInformation($"Processing Resource: {resourceName} with {actionPermissions.Count} actions.");

                // 1. Manage Keycloak Resource
                var existingResource = await _keycloakService.GetResourceAsync(realm, clientUuid.ToString(), keycloakResourceName, adminToken, cancellationToken);

                Guid keycloakResourceId;
                var scopeDtos = actionPermissions.Select(s => new AZM.Abyan.Identity.Application.DTOs.AuthZ.ScopeDto { Name = s.ScopeName }).ToList();
                
                var resourceDto = new AZM.Abyan.Identity.Application.DTOs.AuthZ.ResourceDto
                {
                    Name = keycloakResourceName,
                    DisplayName = $"{resourceName} Controller",
                    Type = "urn:abyan:resource:controller",
                    Uris = new List<string> { $"/{resourceName}" },
                    Scopes = scopeDtos
                };

                if (existingResource == null)
                {
                    _logger.LogInformation($"Creating Keycloak Resource '{keycloakResourceName}'...");
                    keycloakResourceId = await _keycloakService.CreateResourceAsync(realm, clientUuid.ToString(), resourceDto, adminToken, cancellationToken);
                }
                else
                {
                    keycloakResourceId = existingResource.Id.Value;
                    resourceDto.Id = keycloakResourceId;
                    await _keycloakService.UpdateResourceAsync(realm, clientUuid.ToString(), resourceDto, adminToken, cancellationToken);
                }

                // Manage Resource in DB (Deferred until we have a scope)
                var dbResource = await _dbContext.Resources.FirstOrDefaultAsync(r => r.Id == keycloakResourceId, cancellationToken);

                // 2. Manage Scopes, Roles, Policies and Permissions for each Action
                foreach (var permDef in actionPermissions)
                {
                    _logger.LogDebug($"Processing Action: {permDef.ScopeName} (Permission: {permDef.Name})");

                    // a. Ensure Scope exists in DB
                    var dbScope = await _dbContext.Scopes.FirstOrDefaultAsync(s => s.Name == permDef.ScopeName, cancellationToken);
                    if (dbScope == null)
                    {
                        dbScope = new Scope
                        {
                            Id = Guid.NewGuid(),
                            Name = permDef.ScopeName,
                            Description = $"{permDef.ScopeName} scope",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty
                        };
                        _dbContext.Scopes.Add(dbScope);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    // Ensure Resource exists in DB
                    if (dbResource == null)
                    {
                        dbResource = new Resource
                        {
                            Id = keycloakResourceId,
                            Name = resourceName,
                            Description = $"{resourceName} Resource",
                            ScopeId = dbScope.Id,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty
                        };
                        _dbContext.Resources.Add(dbResource);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    // b. Ensure Client Role exists in Keycloak
                    if (!existingRoleNames.Contains(permDef.Name))
                    {
                        _logger.LogInformation($"Creating client role '{permDef.Name}' in Keycloak.");
                        await _keycloakService.CreateClientRoleAsync(realm, clientUuid.ToString(), new CreateClientRoleRequest
                        {
                            Name = permDef.Name,
                            Description = permDef.Description ?? string.Empty
                        }, adminToken, cancellationToken);
                        existingRoleNames.Add(permDef.Name);
                    }

                    // Ensure Role exists in DB
                    var dbRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == permDef.Name, cancellationToken);
                    if (dbRole == null)
                    {
                        dbRole = new Role
                        {
                            Id = Guid.NewGuid(),
                            Name = permDef.Name,
                            Description = permDef.Description ?? string.Empty,
                            ClientId = clientUuid,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty
                        };
                        _dbContext.Roles.Add(dbRole);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    // c. Manage Policy (Role-based) in Keycloak
                    var policyName = $"pol:{resourceName}:{permDef.ScopeName}";
                    var existingPolicy = await _keycloakService.GetPolicyAsync(realm, clientUuid.ToString(), policyName, adminToken, cancellationToken);
                    string keycloakPolicyId;
                    if (existingPolicy == null)
                    {
                        _logger.LogInformation($"Creating Keycloak Policy '{policyName}'...");
                        keycloakPolicyId = await _keycloakService.CreateRolePolicyAsync(realm, clientUuid.ToString(), policyName, new[] { permDef.Name }, adminToken, cancellationToken);
                    }
                    else
                    {
                        keycloakPolicyId = existingPolicy.Id;
                    }

                    // Ensure Policy exists in DB
                    var dbPolicy = await _dbContext.Policies.FirstOrDefaultAsync(p => p.Name == policyName, cancellationToken);
                    if (dbPolicy == null)
                    {
                        dbPolicy = new Policy
                        {
                            Id = Guid.Parse(keycloakPolicyId),
                            Name = policyName,
                            RoleId = dbRole.Id,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty
                        };
                        _dbContext.Policies.Add(dbPolicy);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    // d. Manage Scope-based Permission in Keycloak
                    var authzPermissionName = $"perm:{resourceName}:{permDef.ScopeName}";
                    var existingAuthzPerm = await _keycloakService.GetPermissionAsync(realm, clientUuid.ToString(), authzPermissionName, adminToken, cancellationToken);
                    string keycloakAuthzPermId;
                    if (existingAuthzPerm == null)
                    {
                        _logger.LogInformation($"Creating Keycloak Permission '{authzPermissionName}'...");
                        keycloakAuthzPermId = await _keycloakService.CreateScopePermissionAsync(
                            realm,
                            clientUuid.ToString(),
                            authzPermissionName,
                            [keycloakResourceName],
                            [permDef.ScopeName],
                            [policyName],
                            adminToken,
                            cancellationToken);
                    }
                    else
                    {
                        keycloakAuthzPermId = existingAuthzPerm.Id;
                    }

                    // e. Manage Permission in DB with Keycloak ID
                    var keycloakPermissionIdGuid = Guid.Parse(keycloakAuthzPermId);
                    var dbPermission = await _dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == permDef.Name, cancellationToken);

                    if (dbPermission == null)
                    {
                        _logger.LogInformation($"Adding permission {permDef.Name} to DB with Keycloak ID {keycloakPermissionIdGuid}");
                        dbPermission = new Permission
                        {
                            Id = keycloakPermissionIdGuid,
                            Name = permDef.Name,
                            Description = permDef.Description,
                            ResourceId = dbResource.Id,
                            ScopeId = dbScope.Id,
                            PolicyId = dbPolicy.Id,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty
                        };
                        _dbContext.Permissions.Add(dbPermission);
                    }
                    else if (dbPermission.Id != keycloakPermissionIdGuid)
                    {
                        _logger.LogWarning($"Permission {dbPermission.Name} ID mismatch. Found {dbPermission.Id}, expected {keycloakPermissionIdGuid}. Deleting and recreating...");
                        _dbContext.Permissions.Remove(dbPermission);
                        await _dbContext.SaveChangesAsync(cancellationToken);

                        dbPermission = new Permission
                        {
                            Id = keycloakPermissionIdGuid,
                            Name = permDef.Name,
                            Description = permDef.Description,
                            ResourceId = dbResource.Id,
                            ScopeId = dbScope.Id,
                            PolicyId = dbPolicy.Id,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty
                        };
                        _dbContext.Permissions.Add(dbPermission);
                    }
                    else
                    {
                        dbPermission.ResourceId = dbResource.Id;
                        dbPermission.ScopeId = dbScope.Id;
                        dbPermission.PolicyId = dbPolicy.Id;
                    }
                    
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            _logger.LogInformation("Keycloak sync and database persistence completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing permissions with Keycloak.");
            throw;
        }
    }
}
