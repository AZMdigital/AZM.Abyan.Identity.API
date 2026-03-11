using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Resource.Update;

public class UpdateResourceCommandHandler(
    IRepository<Domain.Entities.Resource, Guid> resourceRepository,
    IRepository<Domain.Entities.Scope, Guid> scopeRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateResourceCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Resource, Guid> _resourceRepository = resourceRepository;
    private readonly IRepository<Domain.Entities.Scope, Guid> _scopeRepository = scopeRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
            if (resource == null)
            {
                return Result<bool>.NotFound(_localizer["ResourceNotFound"]);
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Convert scope names to ScopeDto list
            var scopes = request.UpdateResourceRequest.ScopeNames
                .Select(name => new ScopeDto { Name = name })
                .ToList();

            // Create ResourceDto for Keycloak
            var resourceDto = new ResourceDto
            {
                Id = request.ResourceId,
                Name = request.UpdateResourceRequest.Name,
                DisplayName = request.UpdateResourceRequest.DisplayName ?? request.UpdateResourceRequest.Name,
                Type = request.UpdateResourceRequest.Type,
                Uris = request.UpdateResourceRequest.Uris,
                Scopes = scopes
            };

            // Update resource in Keycloak first
            await _keycloakService.UpdateResourceAsync(
                request.RealmName,
                request.KeycloakClientId,
                resourceDto,
                adminToken,
                cancellationToken);

            // Update resource in database
            resource.Name = request.UpdateResourceRequest.Name;
            resource.Description = request.UpdateResourceRequest.DisplayName ?? request.UpdateResourceRequest.Name;
            
            // Update scope if needed
            if (request.UpdateResourceRequest.ScopeNames.Any())
            {
                var scopeName = request.UpdateResourceRequest.ScopeNames.First();
                var existingScope = await _scopeRepository.GetWhere(s => s.Name == scopeName)
                    .FirstOrDefaultAsync(cancellationToken);
                
                if (existingScope != null)
                {
                    resource.ScopeId = existingScope.Id;
                }
            }

            resource.UpdatedAt = DateTime.UtcNow;
            _resourceRepository.Update(resource);
            await _resourceRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Updated(true, _localizer["ResourceUpdatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
