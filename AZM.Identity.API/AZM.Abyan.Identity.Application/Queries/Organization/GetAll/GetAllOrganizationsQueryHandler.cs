using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetAll;

public class GetAllOrganizationsQueryHandler : IRequestHandler<GetAllOrganizationsQuery, Result<List<OrganizationResponse>>>
{
    private readonly IKeycloakService _keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetAllOrganizationsQueryHandler(
        IKeycloakService keycloakService,
        IStringLocalizer<SharedResource> localizer)
    {
        _keycloakService = keycloakService;
        _localizer = localizer;
    }

    public async Task<Result<List<OrganizationResponse>>> Handle(GetAllOrganizationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var organizations = await _keycloakService.GetOrganizationsAsync(
                request.RealmName,
                adminToken,
                request.Search,
                cancellationToken);

            return Result<List<OrganizationResponse>>.Success(organizations);
        }
        catch (Exception ex)
        {
            return Result<List<OrganizationResponse>>.Failure(ex.Message);
        }
    }
}
