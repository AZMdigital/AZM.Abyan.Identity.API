using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Scope.Delete;

public class DeleteScopeCommandHandler(
    IRepository<Domain.Entities.Scope, Guid> scopeRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeleteScopeCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Scope, Guid> _scopeRepository = scopeRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(DeleteScopeCommand request, CancellationToken cancellationToken)
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

            // Delete scope in Keycloak first
            await _keycloakService.DeleteScopeAsync(
                request.RealmName,
                request.KeycloakClientId,
                request.KeycloakScopeId,
                adminToken,
                cancellationToken);

            // Soft delete scope in database
            scope.SoftDelete();
            _scopeRepository.Update(scope);
            await _scopeRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Deleted(true, _localizer["ScopeDeletedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
