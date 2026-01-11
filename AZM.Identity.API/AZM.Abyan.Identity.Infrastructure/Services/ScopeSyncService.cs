using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class ScopeSyncService : IScopeSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<Scope, Guid> _scopeRepository;
    private readonly IdentityDbContext _dbContext;

    public ScopeSyncService(
        IKeycloakService keycloakService,
        IRepository<Scope, Guid> scopeRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _scopeRepository = scopeRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncScopesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all scopes from Keycloak
            List<AZM.Abyan.Identity.Application.DTOs.AuthZ.ScopeDto> keycloakScopes;
            try
            {
                keycloakScopes = await _keycloakService.GetAllScopesAsync(realm, clientId, adminToken, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                // Client doesn't support scopes or Authorization Services not enabled - skip silently
                return result;
            }

            // Get all scopes from local database
            var localScopes = await _scopeRepository.GetWhere().ToListAsync(cancellationToken);

            // Process each Keycloak scope
            foreach (var keycloakScope in keycloakScopes)
            {
                var localScope = localScopes.FirstOrDefault(s => s.Name == keycloakScope.Name);

                if (localScope == null)
                {
                    // Create new scope
                    localScope = new Scope
                    {
                        Id = Guid.NewGuid(),
                        Name = keycloakScope.Name,
                        Description = keycloakScope.Name, // Use name as description if not provided
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _scopeRepository.CreateAsync(localScope, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing scope
                    localScope.Description = keycloakScope.Name;
                    localScope.UpdatedAt = DateTime.UtcNow;
                    localScope.UpdatedBy = Guid.Empty;
                    _scopeRepository.Update(localScope);
                    result.Updated++;
                }
            }

            // Delete scopes that don't exist in Keycloak
            var keycloakScopeNames = keycloakScopes.Select(s => s.Name).ToHashSet();
            var scopesToDelete = localScopes
                .Where(s => !keycloakScopeNames.Contains(s.Name))
                .ToList();

            foreach (var scopeToDelete in scopesToDelete)
            {
                _dbContext.Scopes.Remove(scopeToDelete);
                result.Deleted++;
            }

            await _scopeRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing scopes: {ex.Message}");
        }

        return result;
    }
}

