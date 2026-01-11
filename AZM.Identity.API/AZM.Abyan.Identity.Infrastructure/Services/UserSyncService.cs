using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class UserSyncService : IUserSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IdentityDbContext _dbContext;

    public UserSyncService(
        IKeycloakService keycloakService,
        IRepository<User, Guid> userRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncUsersAsync(string realm, Guid tenantId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all users from Keycloak for this realm
            var keycloakUsers = await _keycloakService.GetUsersAsync(realm, adminToken, cancellationToken);

            // Get all users from local database for this tenant
            var localUsers = await _userRepository.GetWhere(u => u.TenantId == tenantId).ToListAsync(cancellationToken);

            // Create a dictionary of Keycloak users by ID
            var keycloakUsersDict = keycloakUsers.ToDictionary(u => u.Id, u => u);

            // Process each Keycloak user
            foreach (var keycloakUser in keycloakUsers)
            {
                var localUser = localUsers.FirstOrDefault(u => u.KeycloakUserId?.ToString() == keycloakUser.Id);

                if (localUser == null)
                {
                    // Create new user
                    localUser = new User
                    {
                        Id = Guid.NewGuid(),
                        KeycloakUserId = Guid.TryParse(keycloakUser.Id, out var userId) ? userId : null,
                        Username = keycloakUser.Username,
                        Email = keycloakUser.Email,
                        Firstname = keycloakUser.FirstName,
                        Lastname = keycloakUser.LastName,
                        TenantId = tenantId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _userRepository.CreateAsync(localUser, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing user
                    localUser.Username = keycloakUser.Username;
                    localUser.Email = keycloakUser.Email;
                    localUser.Firstname = keycloakUser.FirstName;
                    localUser.Lastname = keycloakUser.LastName;
                    if (Guid.TryParse(keycloakUser.Id, out var userId))
                    {
                        localUser.KeycloakUserId = userId;
                    }
                    localUser.UpdatedAt = DateTime.UtcNow;
                    localUser.UpdatedBy = Guid.Empty;
                    _userRepository.Update(localUser);
                    result.Updated++;
                }
            }

            // Delete users that don't exist in Keycloak
            var keycloakUserIds = keycloakUsers.Select(u => u.Id).ToHashSet();
            var usersToDelete = localUsers
                .Where(u => u.KeycloakUserId.HasValue && !keycloakUserIds.Contains(u.KeycloakUserId.Value.ToString()))
                .ToList();

            foreach (var userToDelete in usersToDelete)
            {
                _dbContext.Users.Remove(userToDelete);
                result.Deleted++;
            }

            await _userRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing users: {ex.Message}");
        }

        return result;
    }
}

