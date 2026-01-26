using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class PermissionKeycloakSyncService : IPermissionKeycloakSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<Permission, Guid> _permissionRepository;
    private readonly IdentityDbContext _dbContext;

    public PermissionKeycloakSyncService(
        IKeycloakService keycloakService,
        IRepository<Permission, Guid> permissionRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _permissionRepository = permissionRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncPermissionsAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all roles from Keycloak (permissions are now roles)
            List<ClientRoleResponse> keycloakRoles;
            try
            {
                keycloakRoles = await _keycloakService.GetClientRolesAsync(realm, clientId, adminToken, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                // Client doesn't support roles - skip silently
                return result;
            }

            // Get all permissions from local database
            var localPermissions = await _permissionRepository.GetWhere().ToListAsync(cancellationToken);

            // Process each Keycloak role (permission)
            foreach (var keycloakRole in keycloakRoles)
            {
                if (string.IsNullOrEmpty(keycloakRole.Id) || !Guid.TryParse(keycloakRole.Id, out var keycloakRoleId))
                {
                    result.Errors.Add($"Role '{keycloakRole.Name}' has invalid or missing ID, skipping");
                    continue;
                }

                // Extract Controller and Action from role attributes
                string? controller = null;
                string? action = null;

                if (keycloakRole.Attributes != null)
                {
                    if (keycloakRole.Attributes.TryGetValue("Controller", out var controllerValues) && controllerValues != null && controllerValues.Length > 0)
                    {
                        controller = controllerValues[0];
                    }

                    if (keycloakRole.Attributes.TryGetValue("Action", out var actionValues) && actionValues != null && actionValues.Length > 0)
                    {
                        action = actionValues[0];
                    }
                }

                // Only sync roles that have Controller attribute (these are permissions)
                if (string.IsNullOrEmpty(controller))
                {
                    continue; // Skip roles that don't have Controller attribute (not permissions)
                }

                var localPermission = localPermissions.FirstOrDefault(p => p.Id == keycloakRoleId);

                if (localPermission == null)
                {
                    // Create new permission
                    localPermission = new Permission
                    {
                        Id = keycloakRoleId,
                        Name = keycloakRole.Name,
                        Description = keycloakRole.Description ?? string.Empty,
                        Controller = controller,
                        Action = action,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _permissionRepository.CreateAsync(localPermission, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing permission
                    localPermission.Name = keycloakRole.Name;
                    localPermission.Description = keycloakRole.Description ?? string.Empty;
                    localPermission.Controller = controller;
                    localPermission.Action = action;
                    localPermission.UpdatedAt = DateTime.UtcNow;
                    localPermission.UpdatedBy = Guid.Empty;
                    _permissionRepository.Update(localPermission);
                    result.Updated++;
                }
            }

            // Delete permissions that don't exist in Keycloak
            var keycloakRoleIds = keycloakRoles
                .Where(r => !string.IsNullOrEmpty(r.Id) && Guid.TryParse(r.Id, out _))
                .Select(r => Guid.Parse(r.Id))
                .ToHashSet();
            var permissionsToDelete = localPermissions
                .Where(p => !keycloakRoleIds.Contains(p.Id))
                .ToList();

            foreach (var permissionToDelete in permissionsToDelete)
            {
                _dbContext.Permissions.Remove(permissionToDelete);
                result.Deleted++;
            }

            await _permissionRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing permissions: {ex.Message}");
        }

        return result;
    }
}

