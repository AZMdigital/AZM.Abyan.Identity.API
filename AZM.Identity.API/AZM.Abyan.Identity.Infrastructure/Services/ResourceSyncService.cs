using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class ResourceSyncService(
    IKeycloakService keycloakService,
    IRepository<Resource, Guid> resourceRepository,
    IRepository<Scope, Guid> scopeRepository,
    IdentityDbContext dbContext) : IResourceSyncService
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IRepository<Resource, Guid> _resourceRepository = resourceRepository;
    private readonly IRepository<Scope, Guid> _scopeRepository = scopeRepository;
    private readonly IdentityDbContext _dbContext = dbContext;

    public async Task<SyncEntityResult> SyncResourcesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all resources from Keycloak
            List<AZM.Abyan.Identity.Application.DTOs.AuthZ.ResourceDto> keycloakResources;
            try
            {
                keycloakResources = await _keycloakService.GetAllResourcesAsync(realm, clientId, adminToken, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                // Client doesn't support resources or Authorization Services not enabled - skip silently
                return result;
            }

            // Get all resources from local database
            var localResources = await _resourceRepository.GetWhere().ToListAsync(cancellationToken);

            // Get all scopes for matching
            var allScopes = await _scopeRepository.GetWhere().ToListAsync(cancellationToken);

            // Process each Keycloak resource
            foreach (var keycloakResource in keycloakResources)
            {
                if (!keycloakResource.Id.HasValue)
                {
                    result.Errors.Add($"Resource '{keycloakResource.Name}' has no ID from Keycloak, skipping");
                    continue;
                }

                var localResource = localResources.FirstOrDefault(r => r.Id == keycloakResource.Id.Value);

                // Find or create scope (use first scope from resource or create a default one)
                var scopeName = keycloakResource.Scopes?.FirstOrDefault()?.Name ?? "view";
                var scope = allScopes.FirstOrDefault(s => s.Name == scopeName);
                if (scope == null)
                {
                    scope = new Scope
                    {
                        Id = Guid.NewGuid(),
                        Name = scopeName,
                        Description = scopeName,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _scopeRepository.CreateAsync(scope, cancellationToken);
                    allScopes.Add(scope);
                }

                if (localResource == null)
                {
                    // Create new resource
                    localResource = new Resource
                    {
                        Id = keycloakResource.Id.Value,
                        Name = keycloakResource.Name,
                        Description = keycloakResource.DisplayName ?? keycloakResource.Name,
                        ScopeId = scope.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _resourceRepository.CreateAsync(localResource, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing resource
                    localResource.Name = keycloakResource.Name;
                    localResource.Description = keycloakResource.DisplayName ?? keycloakResource.Name;
                    localResource.ScopeId = scope.Id;
                    localResource.UpdatedAt = DateTime.UtcNow;
                    localResource.UpdatedBy = Guid.Empty;
                    _resourceRepository.Update(localResource);
                    result.Updated++;
                }
            }

            // Delete resources that don't exist in Keycloak
            var keycloakResourceIds = keycloakResources
                .Where(r => r.Id.HasValue)
                .Select(r => r.Id!.Value)
                .ToHashSet();
            var resourcesToDelete = localResources
                .Where(r => !keycloakResourceIds.Contains(r.Id))
                .ToList();

            foreach (var resourceToDelete in resourcesToDelete)
            {
                _dbContext.Resources.Remove(resourceToDelete);
                result.Deleted++;
            }

            await _resourceRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing resources: {ex.Message}");
        }

        return result;
    }
}

