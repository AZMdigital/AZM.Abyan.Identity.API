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
             _logger.LogDebug($"Discovered: {p.Name} (Controller: {p.Controller}, Action: {p.Action})");
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

            Guid clientUuid = targetClient.Id;
            _logger.LogInformation($"Syncing with Keycloak Client: {_keycloakConfig.ClientId} (UUID: {clientUuid}) in realm '{realm}'");

            // Ensure Authorization Services are enabled
            //if (!targetClient.AuthorizationServicesEnabled)
            //{
                _logger.LogInformation($"Authorization Services not enabled for client. Enabling now...");
                await _keycloakService.UpdateClientAsync(realm, clientUuid.ToString(), new UpdateClientRequest
                {
                    ClientId = targetClient.ClientId,
                    Name = targetClient.Name,
                    Description = targetClient.Description,
                    Enabled = true,
                    Protocol = "openid-connect",
                    PublicClient = false, // Must be confidential for AuthZ/ServiceAccounts
                    BearerOnly = false,
                    ServiceAccountsEnabled = true, // Required for AuthZ often
                    AuthorizationServicesEnabled = true,
                    RedirectUris = Array.Empty<string>().ToList(),
                    WebOrigins =Array.Empty<string>().ToList()
                }, adminToken, cancellationToken);
                _logger.LogInformation("Authorization Services successfully enabled.");
            //}

            // Get existing client roles ONCE
            var existingRoles = await _keycloakService.GetClientRolesAsync(realm, clientUuid.ToString(), adminToken, cancellationToken);
            var existingRoleNames = existingRoles.Select(r => r.Name).ToHashSet();

            // Group by Controller
            var controllerGroups = permissions.GroupBy(p => p.Controller).ToList();
            _logger.LogInformation($"Grouped into {controllerGroups.Count} controllers.");

            foreach (var group in controllerGroups)
            {
                var controllerName = group.Key;
                var actionPermissions = group.ToList();

                _logger.LogInformation($"Processing Controller: {controllerName} with {actionPermissions.Count} actions.");

                // Process each permission (action) for this controller
                foreach (var permDef in actionPermissions)
                {
                    _logger.LogDebug($"Processing Permission: {permDef.Name} (Controller: {permDef.Controller}, Action: {permDef.Action})");

                    // Get or Add to DB context to track updates
                    var dbPermission = await _dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == permDef.Name, cancellationToken);
                    
                    // Prepare role attributes
                    var attributes = new Dictionary<string, string[]>
                    {
                        ["Controller"] = new[] { permDef.Controller }
                    };

                    if (!string.IsNullOrEmpty(permDef.Action))
                    {
                        attributes["Action"] = new[] { permDef.Action };
                    }

                    // a. Ensure Client Role exists (this is the permission)
                    if (!existingRoleNames.Contains(permDef.Name))
                    {
                        _logger.LogInformation($"Creating permission role '{permDef.Name}' in Keycloak.");
                        await _keycloakService.CreateClientRoleAsync(realm, clientUuid.ToString(), new CreateClientRoleRequest
                        {
                            Name = permDef.Name,
                            Description = permDef.Description ?? string.Empty,
                            Attributes = attributes
                        }, adminToken, cancellationToken);
                        
                        existingRoleNames.Add(permDef.Name);
                    }
                    else
                    {
                        // Update existing role with attributes if needed
                        _logger.LogInformation($"Updating permission role '{permDef.Name}' in Keycloak.");
                        await _keycloakService.UpdateClientRoleAsync(realm, clientUuid.ToString(), permDef.Name, new UpdateClientRoleRequest
                        {
                            Name = permDef.Name,
                            Description = permDef.Description ?? string.Empty,
                            Attributes = attributes
                        }, adminToken, cancellationToken);
                    }

                    // Get the created/updated role from Keycloak to get its ID
                    var keycloakRoles = await _keycloakService.GetClientRolesAsync(realm, clientUuid.ToString(), adminToken, cancellationToken);
                    var createdRole = keycloakRoles.FirstOrDefault(r => r.Name == permDef.Name);

                    if (createdRole == null || string.IsNullOrEmpty(createdRole.Id) || !Guid.TryParse(createdRole.Id, out var keycloakRoleId))
                    {
                        _logger.LogWarning($"Failed to get role ID for permission '{permDef.Name}' from Keycloak");
                        continue;
                    }

                    // Update DB Permission with Keycloak role ID
                    if (dbPermission == null)
                    {
                        dbPermission = new Permission
                        {
                            Id = keycloakRoleId,
                            Name = permDef.Name,
                            Description = permDef.Description ?? string.Empty,
                            Controller = permDef.Controller,
                            Action = permDef.Action,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty
                        };
                        _dbContext.Permissions.Add(dbPermission);
                    }
                    else
                    {
                        // Update existing permission
                        if (dbPermission.Id != keycloakRoleId)
                        {
                            _logger.LogWarning($"Permission {dbPermission.Name} has different ID. Updating from {dbPermission.Id} to {keycloakRoleId}");
                            dbPermission.Id = keycloakRoleId;
                        }
                        dbPermission.Name = permDef.Name;
                        dbPermission.Description = permDef.Description ?? string.Empty;
                        dbPermission.Controller = permDef.Controller;
                        dbPermission.Action = permDef.Action;
                        dbPermission.UpdatedAt = DateTime.UtcNow;
                        dbPermission.UpdatedBy = Guid.Empty;
                    }
                    
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
