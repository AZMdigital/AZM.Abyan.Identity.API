using AZM.Abyan.Identity.Application.DTOs.AuthZ;
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
    private readonly IRepository<Scope, Guid> _scopeRepository;
    private readonly IRepository<Resource, Guid> _resourceRepository;
    private readonly IRepository<Policy, Guid> _policyRepository;
    private readonly IdentityDbContext _dbContext;

    public PermissionKeycloakSyncService(
        IKeycloakService keycloakService,
        IRepository<Permission, Guid> permissionRepository,
        IRepository<Scope, Guid> scopeRepository,
        IRepository<Resource, Guid> resourceRepository,
        IRepository<Policy, Guid> policyRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _permissionRepository = permissionRepository;
        _scopeRepository = scopeRepository;
        _resourceRepository = resourceRepository;
        _policyRepository = policyRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncPermissionsAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all permissions from Keycloak
            List<AZM.Abyan.Identity.Application.DTOs.AuthZ.PermissionDto> keycloakPermissions;
            try
            {
                keycloakPermissions = await _keycloakService.GetAllPermissionsAsync(realm, clientId, adminToken, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                // Client doesn't support permissions or Authorization Services not enabled - skip silently
                return result;
            }

            // Get all permissions from local database
            var localPermissions = await _permissionRepository.GetWhere().ToListAsync(cancellationToken);

            // Get all related entities
            var allScopes = await _scopeRepository.GetWhere().ToListAsync(cancellationToken);
            var allResources = await _resourceRepository.GetWhere().ToListAsync(cancellationToken);
            var allPolicies = await _policyRepository.GetWhere().ToListAsync(cancellationToken);

            // Process each Keycloak permission (only scope permissions)
            foreach (var keycloakPermission in keycloakPermissions.Where(p => p.Type == "scope"))
            {
                var keycloakPermissionIdGuid = Guid.TryParse(keycloakPermission.Id, out var permId) ? (Guid?)permId : null;
                var localPermission = localPermissions.FirstOrDefault(p => p.KeycloakPermissionId == keycloakPermissionIdGuid);

                // Find scope (use first scope from permission)
                var scopeName = keycloakPermission.Scopes?.FirstOrDefault();
                var scope = scopeName != null ? allScopes.FirstOrDefault(s => s.Name == scopeName) : null;

                // Find resource (use first resource from permission)
                var resourceName = keycloakPermission.Resources?.FirstOrDefault();
                var resource = resourceName != null ? allResources.FirstOrDefault(r => r.Name == resourceName) : null;

                // Find policy (use first policy from permission)
                var policyName = keycloakPermission.Policies?.FirstOrDefault();
                var policy = policyName != null ? allPolicies.FirstOrDefault(p => p.Name == policyName) : null;

                // Skip if required entities are missing
                if (scope == null || resource == null || policy == null)
                {
                    result.Errors.Add($"Permission '{keycloakPermission.Name}' is missing required entities (scope, resource, or policy), skipping");
                    continue;
                }

                if (localPermission == null)
                {
                    // Create new permission
                    localPermission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = keycloakPermission.Name,
                        Description = keycloakPermission.Name,
                        KeycloakPermissionId = keycloakPermissionIdGuid,
                        ScopeId = scope.Id,
                        ResourceId = resource.Id,
                        PolicyId = policy.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _permissionRepository.CreateAsync(localPermission, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing permission
                    localPermission.Name = keycloakPermission.Name;
                    localPermission.KeycloakPermissionId = keycloakPermissionIdGuid;
                    localPermission.ScopeId = scope.Id;
                    localPermission.ResourceId = resource.Id;
                    localPermission.PolicyId = policy.Id;
                    localPermission.UpdatedAt = DateTime.UtcNow;
                    localPermission.UpdatedBy = Guid.Empty;
                    _permissionRepository.Update(localPermission);
                    result.Updated++;
                }
            }

            // Delete permissions that don't exist in Keycloak
            var keycloakPermissionIds = keycloakPermissions
                .Select(p => Guid.TryParse(p.Id, out var id) ? (Guid?)id : null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            var permissionsToDelete = localPermissions
                .Where(p => p.KeycloakPermissionId.HasValue && !keycloakPermissionIds.Contains(p.KeycloakPermissionId.Value))
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

