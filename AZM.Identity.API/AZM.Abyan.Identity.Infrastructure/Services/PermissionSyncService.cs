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
using System.Reflection;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class PermissionSyncService(
    IdentityDbContext dbContext,
    IKeycloakService keycloakService,
    IOptions<KeycloakConfiguration> keycloakConfig,
    ILogger<PermissionSyncService> logger) : IPermissionSyncService
{
    private readonly IdentityDbContext _dbContext = dbContext;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly KeycloakConfiguration _keycloakConfig = keycloakConfig.Value;
    private readonly ILogger<PermissionSyncService> _logger = logger;

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
            if (clients == null || !clients.Any())
            {
                _logger.LogError("No clients found in Keycloak realm '{realm}'.", realm);
                return;
            }

            var targetClient = clients.FirstOrDefault(c => c.ClientId == _keycloakConfig.ClientId);

            if (targetClient == null)
            {
                _logger.LogError("Client with ClientId '{ClientId}' not found in Keycloak realm '{Realm}'.", _keycloakConfig.ClientId, realm);
                return;
            }

            Guid clientUuid = targetClient.Id;
            _logger.LogInformation("Syncing with Keycloak Client: {ClientId} (UUID: {ClientUuid}) in realm '{Realm}'", _keycloakConfig.ClientId, clientUuid, realm);

            // Ensure Authorization Services are enabled
            _logger.LogInformation("Authorization Services check for client...");
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
            if (existingKeycloakRoles == null)
            {
                _logger.LogError("Failed to retrieve existing client roles from Keycloak.");
                return;
            }

            var existingRoleNames = existingKeycloakRoles.Select(r => r.Name).ToHashSet();

            // Group by Resource
            var resourceGroups = permissions?.GroupBy(p => p.ResourceName).ToList() ?? new List<IGrouping<string, DiscoveredPermission>>();
            _logger.LogInformation("Grouped into {ResourceCount} resources.", resourceGroups.Count);

            foreach (var group in resourceGroups)
            {
                var resourceName = group.Key;
                var actionPermissions = group.ToList();
                var keycloakResourceName = $"res:{resourceName}";

                _logger.LogInformation("Processing Resource: {ResourceName} with {ActionCount} actions.", resourceName, actionPermissions.Count);

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
                    _logger.LogInformation("Creating Keycloak Resource '{KeycloakResourceName}'...", keycloakResourceName);
                    keycloakResourceId = await _keycloakService.CreateResourceAsync(realm, clientUuid.ToString(), resourceDto, adminToken, cancellationToken);
                }
                else
                {
                    keycloakResourceId = existingResource.Id ?? Guid.NewGuid();
                    resourceDto.Id = keycloakResourceId;
                    await _keycloakService.UpdateResourceAsync(realm, clientUuid.ToString(), resourceDto, adminToken, cancellationToken);
                }

                // Manage Resource in DB (Deferred until we have a scope)
                var dbResource = await _dbContext.Resources.FirstOrDefaultAsync(r => r.Id == keycloakResourceId, cancellationToken);

                // 2. Manage Scopes, Roles, Policies and Permissions for each Action
                foreach (var permDef in actionPermissions)
                {
                    _logger.LogDebug("Processing Action: {ScopeName} (Permission: {PermissionName})", permDef.ScopeName, permDef.Name);

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
                        _logger.LogInformation("Creating client role '{RoleName}' in Keycloak.", permDef.Name);
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
                        _logger.LogInformation("Creating Keycloak Policy '{PolicyName}'...", policyName);
                        keycloakPolicyId = await _keycloakService.CreateRolePolicyAsync(realm, clientUuid.ToString(), policyName, new[] { permDef.Name }, adminToken, cancellationToken);
                    }
                    else
                    {
                        keycloakPolicyId = existingPolicy.Id!;
                    }

                    // Ensure Policy exists in DB
                    var dbPolicy = await _dbContext.Policies.FirstOrDefaultAsync(p => p.Name == policyName, cancellationToken);
                    if (dbPolicy == null)
                    {
                        dbPolicy = new Policy
                        {
                            Id = Guid.TryParse(keycloakPolicyId, out var policyGuid) ? policyGuid : Guid.NewGuid(),
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
                        _logger.LogInformation("Creating Keycloak Permission '{PermissionName}'...", authzPermissionName);
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
                        keycloakAuthzPermId = existingAuthzPerm.Id!;
                    }

                    // e. Manage Permission in DB with Keycloak ID
                    if (Guid.TryParse(keycloakAuthzPermId, out var keycloakPermissionIdGuid))
                    {
                        var dbPermission = await _dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == permDef.Name, cancellationToken);

                        if (dbPermission == null)
                        {
                            _logger.LogInformation("Adding permission {PermissionName} to DB with Keycloak ID {KeycloakPermissionId}", permDef.Name, keycloakPermissionIdGuid);
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
                            _logger.LogWarning("Permission {PermissionName} ID mismatch. Found {ExistingId}, expected {ExpectedId}. Deleting and recreating...", dbPermission.Name, dbPermission.Id, keycloakPermissionIdGuid);
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
                    else
                    {
                        _logger.LogError("Failed to parse Keycloak Permission ID. Permission sync aborted for {PermissionName}.", permDef.Name);
                    }
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
