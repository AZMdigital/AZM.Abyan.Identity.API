using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class UserPermissionQueryService(IdentityDbContext dbContext) : IUserPermissionQueryService
{
    private readonly IdentityDbContext _dbContext = dbContext;

    public async Task<List<PermissionDto>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userGuid = Guid.Parse(userId);
        // Get user's role IDs
        var userRoleIds = await _dbContext.TenantUserRoles
            .Where(tur => tur.UserId == userGuid)
            .Select(tur => tur.RoleId)
            .ToListAsync(cancellationToken);

        // Get permissions where Permission.Policy.RoleId is in user's roles
        return await _dbContext.Permissions
            .Where(p => userRoleIds.Contains(p.Policy.RoleId))
            .Select(p => new PermissionDto
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                Type = p.Scope != null ? "scope" : "resource",
                Logic = "POSITIVE",
                DecisionStrategy = "UNANIMOUS",
                Resources = new List<string> { p.Resources.Name },
                Scopes = p.Scope != null ? new List<string> { p.Scope.Name } : new List<string>(),
                Policies = new List<string> { p.Policy.Name }
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PolicyDto>> GetUserPoliciesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userGuid = Guid.Parse(userId);
        var userRoleIds = await _dbContext.TenantUserRoles
            .Where(tur => tur.UserId == userGuid)
            .Select(tur => tur.RoleId)
            .ToListAsync(cancellationToken);
        return await _dbContext.Policies
            .Where(pol => userRoleIds.Contains(pol.RoleId))
            .Select(pol => new PolicyDto
            {
                Id = pol.Id.ToString(),
                Name = pol.Name,
                Type = "role",
                Logic = "POSITIVE",
                DecisionStrategy = "UNANIMOUS",
                Config = new Dictionary<string, object>()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ResourceDto>> GetUserResourcesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userGuid = Guid.Parse(userId);
        var userRoleIds = await _dbContext.TenantUserRoles
            .Where(tur => tur.UserId == userGuid)
            .Select(tur => tur.RoleId)
            .ToListAsync(cancellationToken);
        var resourceIds = await _dbContext.Permissions
            .Where(p => userRoleIds.Contains(p.Policy.RoleId))
            .Select(p => p.ResourceId)
            .Distinct()
            .ToListAsync(cancellationToken);
        return await _dbContext.Resources
            .Where(r => resourceIds.Contains(r.Id))
            .Select(r => new ResourceDto
            {
                Id = r.Id,
                Name = r.Name,
                DisplayName = r.Description,
                Scopes = new List<ScopeDto> {
                    new ScopeDto { Id = r.Scope.Id.ToString(), Name = r.Scope.Name }
                },
                Type = "urn:resource:api",
                Uris = new List<string>()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ScopeDto>> GetUserScopesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userGuid = Guid.Parse(userId);
        var userRoleIds = await _dbContext.TenantUserRoles
            .Where(tur => tur.UserId == userGuid)
            .Select(tur => tur.RoleId)
            .ToListAsync(cancellationToken);
        var scopeIds = await _dbContext.Permissions
            .Where(p => userRoleIds.Contains(p.Policy.RoleId))
            .Select(p => p.ScopeId)
            .Distinct()
            .ToListAsync(cancellationToken);
        return await _dbContext.Scopes
            .Where(s => scopeIds.Contains(s.Id))
            .Select(s => new ScopeDto
            {
                Id = s.Id.ToString(),
                Name = s.Name
            })
            .ToListAsync(cancellationToken);
    }
}
