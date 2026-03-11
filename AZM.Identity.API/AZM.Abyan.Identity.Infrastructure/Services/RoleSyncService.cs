using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class RoleSyncService(
    IKeycloakService keycloakService,
    IRepository<Role, Guid> roleRepository,
    IdentityDbContext dbContext) : IRoleSyncService
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IRepository<Role, Guid> _roleRepository = roleRepository;
    private readonly IdentityDbContext _dbContext = dbContext;

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
                if (!Guid.TryParse(keycloakRole.Id, out var keycloakRoleId))
                {
                    result.Errors.Add($"Invalid Keycloak role ID format: {keycloakRole.Id}");
                    continue;
                }

                var localRole = localRoles.FirstOrDefault(r => r.Id == keycloakRoleId);

                if (localRole == null)
                {
                    // Create new role
                    localRole = new Role
                    {
                        Id = keycloakRoleId,
                        Name = keycloakRole.Name,
                        Description = keycloakRole.Description ?? string.Empty,
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
                    localRole.UpdatedAt = DateTime.UtcNow;
                    localRole.UpdatedBy = Guid.Empty;
                    _roleRepository.Update(localRole);
                    result.Updated++;
                }
            }

            // Delete roles that don't exist in Keycloak
            var keycloakRoleIds = keycloakRoles
                .Where(r => Guid.TryParse(r.Id, out _))
                .Select(r => Guid.Parse(r.Id))
                .ToHashSet();
            var rolesToDelete = localRoles
                .Where(r => !keycloakRoleIds.Contains(r.Id))
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

