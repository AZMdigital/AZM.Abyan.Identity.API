using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Resource.GetResources;

public class GetResourcesQueryHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetResourcesQuery, Result<object>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<object>> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var resources = await _keycloakService.GetAllResourcesAsync(request.RealmName, request.ClientId, adminToken, cancellationToken);
        return Result<object>.Success(resources, _localizer["OperationSuccess"]);
    }
}
