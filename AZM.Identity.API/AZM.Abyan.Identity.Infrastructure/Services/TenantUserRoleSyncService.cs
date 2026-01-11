using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class TenantUserRoleSyncService : ITenantUserRoleSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<TenantUserRole, Guid> _tenantUserRoleRepository;
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IRepository<Role, Guid> _roleRepository;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IdentityDbContext _dbContext;

    public TenantUserRoleSyncService(
        IKeycloakService keycloakService,
        IRepository<TenantUserRole, Guid> tenantUserRoleRepository,
        IRepository<User, Guid> userRepository,
        IRepository<Role, Guid> roleRepository,
        IRepository<Tenant, Guid> tenantRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _tenantUserRoleRepository = tenantUserRoleRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _tenantRepository = tenantRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncTenantUserRolesAsync(string realm, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get tenant
            var tenant = await _tenantRepository.GetWhere(t => t.Name == realm).FirstOrDefaultAsync(cancellationToken);
            if (tenant == null)
            {
                result.Errors.Add($"Tenant '{realm}' not found");
                return result;
            }

            // Get all users from Keycloak
            var keycloakUsers = await _keycloakService.GetUsersAsync(realm, adminToken, cancellationToken);

            // Get all local users for this tenant
            var localUsers = await _userRepository.GetWhere(u => u.TenantId == tenant.Id).ToListAsync(cancellationToken);

            // Get all roles
            var allRoles = await _roleRepository.GetWhere().ToListAsync(cancellationToken);

            // Get all existing tenant user roles
            var existingTenantUserRoles = await _tenantUserRoleRepository.GetWhere(tur => tur.TenantId == tenant.Id).ToListAsync(cancellationToken);

            // Build a set of Keycloak user-role assignments
            var keycloakAssignments = new HashSet<(Guid UserId, Guid RoleId)>();

            foreach (var keycloakUser in keycloakUsers)
            {
                var localUser = localUsers.FirstOrDefault(u => u.KeycloakUserId?.ToString() == keycloakUser.Id);
                if (localUser == null) continue;

                // Get user's client roles
                var clients = await _keycloakService.GetClientsAsync(realm, adminToken, cancellationToken);
                foreach (var client in clients)
                {
                    List<AZM.Abyan.Identity.Application.DTOs.Roles.ClientRoleResponse> clientRoles;
                    try
                    {
                        clientRoles = await _keycloakService.GetUserClientRolesAsync(keycloakUser.Id, client.Id, adminToken, cancellationToken);
                    }
                    catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                    {
                        // Client doesn't support roles or user has no roles - skip silently
                        continue;
                    }
                    foreach (var clientRole in clientRoles)
                    {
                        var role = allRoles.FirstOrDefault(r => r.KeycloakRoleId?.ToString() == clientRole.Id && r.ClientId.ToString() == client.Id);
                        if (role != null)
                        {
                            keycloakAssignments.Add((localUser.Id, role.Id));
                        }
                    }
                }
            }

            // Create new assignments
            foreach (var (userId, roleId) in keycloakAssignments)
            {
                var exists = existingTenantUserRoles.Any(tur => tur.UserId == userId && tur.RoleId == roleId);
                if (!exists)
                {
                    var tenantUserRole = new TenantUserRole
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenant.Id,
                        UserId = userId,
                        RoleId = roleId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _tenantUserRoleRepository.CreateAsync(tenantUserRole, cancellationToken);
                    result.Added++;
                }
            }

            // Delete assignments that don't exist in Keycloak
            var assignmentsToDelete = existingTenantUserRoles
                .Where(tur => !keycloakAssignments.Contains((tur.UserId ?? Guid.Empty, tur.RoleId)))
                .ToList();

            foreach (var assignmentToDelete in assignmentsToDelete)
            {
                _dbContext.TenantUserRoles.Remove(assignmentToDelete);
                result.Deleted++;
            }

            await _tenantUserRoleRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing tenant user roles: {ex.Message}");
        }

        return result;
    }
}

