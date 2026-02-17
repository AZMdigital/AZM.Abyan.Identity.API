using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Delete;

public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Result<bool>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteOrganizationCommandHandler(
        ITenantRepository tenantRepository,
        IKeycloakService keycloakService,
        IStringLocalizer<SharedResource> localizer)
    {
        _tenantRepository = tenantRepository;
        _keycloakService = keycloakService;
        _localizer = localizer;
    }

    public async Task<Result<bool>> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get tenant from database
            var tenant = await _tenantRepository.GetByIdAsync(request.Id, cancellationToken);
            if (tenant == null)
            {
                return Result<bool>.NotFound(_localizer["OrganizationNotFound"]);
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Delete from Keycloak
            await _keycloakService.DeleteOrganizationAsync(
                request.RealmName,
                request.Id.ToString(),
                adminToken,
                cancellationToken);

            // Soft delete from database
            var deleted = await _tenantRepository.DeleteAsync(request.Id, cancellationToken);
            if (!deleted)
            {
                return Result<bool>.NotFound(_localizer["OrganizationNotFound"]);
            }

            return Result<bool>.Deleted(true, _localizer["OrganizationDeletedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
