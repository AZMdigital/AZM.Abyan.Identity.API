using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Scope.Create;

public class CreateScopeCommandHandler(
    IRepository<Domain.Entities.Scope, Guid> scopeRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateScopeCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.Scope, Guid> _scopeRepository = scopeRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateScopeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Create ScopeDto for Keycloak
            var scopeDto = new ScopeDto
            {
                Name = request.CreateScopeRequest.Name
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

            // Check if createdScope is null
            if (createdScope == null || string.IsNullOrEmpty(createdScope.Id))
            {
                return Result<Guid>.Failure(_localizer["FailedToRetrieveCreatedScope"]);
            }

            // Create local entity
            if (!Guid.TryParse(createdScope.Id, out var scopeId))
            {
                return Result<Guid>.Failure(_localizer["InvalidScopeId"]);
            }

            var scope = new Domain.Entities.Scope
            {
                Id = scopeId,
                Name = request.CreateScopeRequest.Name,
                Description = request.CreateScopeRequest.Name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _scopeRepository.CreateAsync(scope, cancellationToken);
            await _scopeRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(scope.Id, _localizer["ScopeCreatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
