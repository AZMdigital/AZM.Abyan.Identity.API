using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Resource.GetResourceById;

public class GetResourceByIdQueryHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetResourceByIdQuery, Result<object>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<object>> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var resource = await _keycloakService.GetResourceByIdAsync(request.RealmName, request.ClientId, request.ResourceId, adminToken, cancellationToken);

        if (resource == null)
            return Result<object>.NotFound(_localizer["ResourceNotFound"]);

        return Result<object>.Success(resource, _localizer["OperationSuccess"]);
    }
}
