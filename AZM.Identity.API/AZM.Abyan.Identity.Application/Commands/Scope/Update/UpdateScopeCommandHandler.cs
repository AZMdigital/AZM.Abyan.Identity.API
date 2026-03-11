using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Scope.Update;

public class UpdateScopeCommandHandler(
    IRepository<Domain.Entities.Scope, Guid> scopeRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateScopeCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Scope, Guid> _scopeRepository = scopeRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(UpdateScopeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scope = await _scopeRepository.GetByIdAsync(request.ScopeId, cancellationToken);
            if (scope == null)
            {
                return Result<bool>.NotFound(_localizer["ScopeNotFound"]);
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Create ScopeDto for Keycloak
            var scopeDto = new ScopeUpdateDto
            {
                Id=request.KeycloakScopeId,
                DisplayName=request.UpdateScopeRequest.Name,
                Name = request.UpdateScopeRequest.Name
            };

            // Update scope in Keycloak first
            await _keycloakService.UpdateScopeAsync(
                request.RealmName,
                request.KeycloakClientId,
                request.KeycloakScopeId,
                scopeDto,
                adminToken,
                cancellationToken);

            // Update scope in database
            scope.Name = request.UpdateScopeRequest.Name;
            scope.Description = request.UpdateScopeRequest.Name;
            scope.UpdatedAt = DateTime.UtcNow;
            _scopeRepository.Update(scope);
            await _scopeRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Updated(true, _localizer["ScopeUpdatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
