using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Resource.Create;

public class CreateResourceCommandHandler(
    IRepository<Domain.Entities.Resource, Guid> resourceRepository,
    IRepository<Domain.Entities.Scope, Guid> scopeRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateResourceCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.Resource, Guid> _resourceRepository = resourceRepository;
    private readonly IRepository<Domain.Entities.Scope, Guid> _scopeRepository = scopeRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Convert scope names to ScopeDto list
            var scopes = request.CreateResourceRequest.ScopeNames
                .Select(name => new ScopeDto { Name = name })
                .ToList();

            // Create ResourceDto for Keycloak
            var resourceDto = new ResourceDto
            {
                Name = request.CreateResourceRequest.Name,
                DisplayName = request.CreateResourceRequest.DisplayName ?? request.CreateResourceRequest.Name,
                Type = request.CreateResourceRequest.Type,
                Uris = request.CreateResourceRequest.Uris,
                Scopes = scopes
            };

            // Create resource in Keycloak first
            var keycloakResourceId = await _keycloakService.CreateResourceAsync(
                request.RealmName,
                request.KeycloakClientId,
                resourceDto,
                adminToken,
                cancellationToken);

            if (keycloakResourceId == Guid.Empty)
            {
                return Result<Guid>.Failure(_localizer["FailedToCreateResourceInKeycloak"]);
            }

            // Get or create scope in database (assuming first scope for now)
            Guid scopeId = Guid.Empty;
            if (request.CreateResourceRequest.ScopeNames.Any())
            {
                var scopeName = request.CreateResourceRequest.ScopeNames.First();
                var existingScope = await _scopeRepository.GetWhere(s => s.Name == scopeName)
                    .FirstOrDefaultAsync(cancellationToken);


                if (existingScope == null)
                {
                    // Create ScopeDto for Keycloak
                    var scopeDto = new ScopeDto
                    {
                        Name = request.CreateResourceRequest.ScopeNames.First()
                    };

                    // Create scope in Keycloak first
                    var keycloakScopeName = await _keycloakService.CreateScopeAsync(
                        request.RealmName,
                        request.KeycloakClientId,
                        scopeDto,
                        adminToken,
                        cancellationToken);

                    if (string.IsNullOrEmpty(keycloakScopeName))
                    {
                        return Result<Guid>.Failure(_localizer["FailedToCreateScopeInKeycloak"]);
                    }

                    // Get the created scope from Keycloak to get its ID
                    var createdScope = await _keycloakService.GetScopeAsync(
                        request.RealmName,
                        request.KeycloakClientId,
                        keycloakScopeName,
                        adminToken,
                        cancellationToken);
                    // Check if createdScope or its Id is null
                    if (createdScope == null || string.IsNullOrEmpty(createdScope.Id))
                    {
                        return Result<Guid>.Failure(_localizer["FailedToRetrieveCreatedScope"]);
                    }

                    // Parse the scope ID safely
                    if (!Guid.TryParse(createdScope.Id, out var parsedScopeId))
                    {
                        return Result<Guid>.Failure(_localizer["InvalidScopeId"]);
                    }

                    // Create local entity
                    var scope = new Domain.Entities.Scope
                    {
                        Id = parsedScopeId,
                        Name = request.CreateResourceRequest.ScopeNames.First(),
                        Description = request.CreateResourceRequest.ScopeNames.First(),
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _scopeRepository.CreateAsync(scope, cancellationToken);
                    await _scopeRepository.SaveChangesAsync(cancellationToken);
                    scopeId = scope.Id;
                }
                else
                {
                    scopeId = existingScope.Id;
                }
            }

            // Create local entity with ID from Keycloak
            var resource = new Domain.Entities.Resource
            {
                Id = keycloakResourceId,
                Name = request.CreateResourceRequest.Name,
                Description = request.CreateResourceRequest.DisplayName ?? request.CreateResourceRequest.Name,
                ScopeId = scopeId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _resourceRepository.CreateAsync(resource, cancellationToken);
            await _resourceRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(resource.Id, _localizer["ResourceCreatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
