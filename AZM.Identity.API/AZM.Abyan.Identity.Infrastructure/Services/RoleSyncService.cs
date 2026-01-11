using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class RoleSyncService : IRoleSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<Role, Guid> _roleRepository;
    private readonly IdentityDbContext _dbContext;

    public RoleSyncService(
        IKeycloakService keycloakService,
        IRepository<Role, Guid> roleRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _roleRepository = roleRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncRolesAsync(string realm, string keycloakClientId, Guid localClientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all client roles from Keycloak (use Keycloak client ID)
            List<AZM.Abyan.Identity.Application.DTOs.Roles.ClientRoleResponse> keycloakRoles;
            try
            {
                keycloakRoles = await _keycloakService.GetClientRolesAsync(realm, keycloakClientId, adminToken, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                // Client doesn't support roles or Authorization Services not enabled - skip silently
                return result;
            }

            // Get all roles from local database for this client (use local client ID)
            var localRoles = await _roleRepository.GetWhere(r => r.ClientId == localClientId).ToListAsync(cancellationToken);

            // Process each Keycloak role
            foreach (var keycloakRole in keycloakRoles)
            {
                var localRole = localRoles.FirstOrDefault(r => r.KeycloakRoleId?.ToString() == keycloakRole.Id);

                if (localRole == null)
                {
                    // Create new role
                    localRole = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = keycloakRole.Name,
                        Description = keycloakRole.Description ?? string.Empty,
                        KeycloakRoleId = Guid.TryParse(keycloakRole.Id, out var roleId) ? roleId : null,
                        ClientId = localClientId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _roleRepository.CreateAsync(localRole, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing role
                    localRole.Name = keycloakRole.Name;
                    localRole.Description = keycloakRole.Description ?? string.Empty;
                    if (Guid.TryParse(keycloakRole.Id, out var roleId))
                    {
                        localRole.KeycloakRoleId = roleId;
                    }
                    localRole.UpdatedAt = DateTime.UtcNow;
                    localRole.UpdatedBy = Guid.Empty;
                    _roleRepository.Update(localRole);
                    result.Updated++;
                }
            }

            // Delete roles that don't exist in Keycloak
            var keycloakRoleIds = keycloakRoles.Select(r => r.Id).ToHashSet();
            var rolesToDelete = localRoles
                .Where(r => r.KeycloakRoleId.HasValue && !keycloakRoleIds.Contains(r.KeycloakRoleId.Value.ToString()))
                .ToList();

            foreach (var roleToDelete in rolesToDelete)
            {
                _dbContext.Roles.Remove(roleToDelete);
                result.Deleted++;
            }

            await _roleRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing roles: {ex.Message}");
        }

        return result;
    }
}

