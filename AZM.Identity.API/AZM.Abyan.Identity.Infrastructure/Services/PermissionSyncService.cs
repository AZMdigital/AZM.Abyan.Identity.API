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
            _logger.LogDebug($"Discovered: {p.Name} (Controller: {p.Controller}, Action: {p.Action})");
        }

        // 2. Sync with Keycloak (Now includes DB sync with Keycloak IDs)
        await SyncWithKeycloakAsync(discoveredPermissions, cancellationToken);

        _logger.LogInformation("Permission sync process finished.");
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
            _logger.LogInformation($"Ensuring Authorization Services are enabled for client...");
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
            var existingRoles = await _keycloakService.GetClientRolesAsync(realm, clientUuid.ToString(), adminToken, cancellationToken);
            var existingRoleMap = existingRoles.ToDictionary(r => r.Name, r => r);

            // Process each discovered permission
            foreach (var permDef in permissions)
            {
                _logger.LogDebug($"Processing Permission: {permDef.Name}");

                // Prepare role attributes
                var attributes = new Dictionary<string, string[]>
                {
                    ["Controller"] = new[] { permDef.Controller }
                };

                if (!string.IsNullOrEmpty(permDef.Action))
                {
                    attributes["Action"] = new[] { permDef.Action };
                }

                // a. Ensure Client Role exists in Keycloak
                if (!existingRoleMap.TryGetValue(permDef.Name, out var kcRole))
                {
                    _logger.LogInformation($"Creating permission role '{permDef.Name}' in Keycloak.");
                    await _keycloakService.CreateClientRoleAsync(realm, clientUuid.ToString(), new CreateClientRoleRequest
                    {
                        Name = permDef.Name,
                        Description = permDef.Description ?? string.Empty,
                        Attributes = attributes
                    }, adminToken, cancellationToken);

                    // Re-fetch roles to get the new role with its ID
                    existingRoles = await _keycloakService.GetClientRolesAsync(realm, clientUuid.ToString(), adminToken, cancellationToken);
                    existingRoleMap = existingRoles.ToDictionary(r => r.Name, r => r);
                    kcRole = existingRoleMap[permDef.Name];
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

                if (string.IsNullOrEmpty(kcRole.Id) || !Guid.TryParse(kcRole.Id, out var keycloakRoleId))
                {
                    _logger.LogWarning($"Failed to get valid role ID for permission '{permDef.Name}' from Keycloak");
                    continue;
                }

                // b. Update local database with Keycloak ID
                var dbPermission = await _dbContext.Permissions
                    .FirstOrDefaultAsync(p => p.Name == permDef.Name, cancellationToken);

                if (dbPermission == null)
                {
                    _logger.LogInformation($"Adding new permission '{permDef.Name}' to DB with ID {keycloakRoleId}");
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
                    if (dbPermission.Id != keycloakRoleId)
                    {
                        _logger.LogWarning($"Permission '{dbPermission.Name}' ID mismatch. Updating DB ID from {dbPermission.Id} to {keycloakRoleId}");
                        
                        // EF Core doesn't allow changing PK normally easily. 
                        // If it's the PK, we might need to delete and recreate or use a raw SQL if it's critical.
                        // However, many systems use a separate business key and a serial ID.
                        // Based on BaseEntity, Id is the Key.
                        
                        // Option 1: Delete and recreate
                        _dbContext.Permissions.Remove(dbPermission);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        
                        dbPermission = new Permission
                        {
                            Id = keycloakRoleId,
                            Name = permDef.Name,
                            Description = permDef.Description ?? string.Empty,
                            Controller = permDef.Controller,
                            Action = permDef.Action,
                            CreatedAt = dbPermission.CreatedAt,
                            CreatedBy = dbPermission.CreatedBy,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = Guid.Empty
                        };
                        _dbContext.Permissions.Add(dbPermission);
                    }
                    else
                    {
                        dbPermission.Description = permDef.Description ?? string.Empty;
                        dbPermission.Controller = permDef.Controller;
                        dbPermission.Action = permDef.Action;
                        dbPermission.UpdatedAt = DateTime.UtcNow;
                        dbPermission.UpdatedBy = Guid.Empty;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            _logger.LogInformation("Keycloak & DB Permission sync completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing permissions with Keycloak.");
            throw;
        }
    }
}
