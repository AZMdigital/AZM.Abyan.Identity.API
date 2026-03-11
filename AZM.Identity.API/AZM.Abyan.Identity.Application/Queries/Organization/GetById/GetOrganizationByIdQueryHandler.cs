using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetById;

public class GetOrganizationByIdQueryHandler(
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetOrganizationByIdQuery, Result<OrganizationResponse>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<OrganizationResponse>> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var organization = await _keycloakService.GetOrganizationByIdAsync(
                request.RealmName,
                request.OrganizationId,
                adminToken,
                cancellationToken);

            if (organization == null)
                return Result<OrganizationResponse>.NotFound(_localizer["OrganizationNotFound"]);

            return Result<OrganizationResponse>.Success(organization);
        }
        catch (Exception ex)
        {
            return Result<OrganizationResponse>.Failure(ex.Message);
        }
    }
}
