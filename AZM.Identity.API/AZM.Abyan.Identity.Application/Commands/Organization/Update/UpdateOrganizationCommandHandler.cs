using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Update;

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, Result<bool>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateOrganizationCommandHandler(
        ITenantRepository tenantRepository,
        IKeycloakService keycloakService,
        IStringLocalizer<SharedResource> localizer)
    {
        _tenantRepository = tenantRepository;
        _keycloakService = keycloakService;
        _localizer = localizer;
    }

    public async Task<Result<bool>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
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

            // Update in Keycloak
            await _keycloakService.UpdateOrganizationAsync(
                request.RealmName,
                request.Id.ToString(),
                request.UpdateOrganizationRequest,
                adminToken,
                cancellationToken);

            // Update in database
            if (request.UpdateOrganizationRequest.Name != null)
                tenant.Name = request.UpdateOrganizationRequest.Name;

            if (request.UpdateOrganizationRequest.Enabled.HasValue)
                tenant.IsActive = request.UpdateOrganizationRequest.Enabled.Value;

            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.UpdatedBy = Guid.Empty;

            var updated = await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            if (!updated)
            {
                return Result<bool>.NotFound(_localizer["OrganizationNotFound"]);
            }

            return Result<bool>.Updated(true, _localizer["OrganizationUpdatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
