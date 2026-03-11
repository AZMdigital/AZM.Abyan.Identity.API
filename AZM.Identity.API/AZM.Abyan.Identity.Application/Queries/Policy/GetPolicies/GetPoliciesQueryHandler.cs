using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Policy.GetPolicies;

public class GetPoliciesQueryHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetPoliciesQuery, Result<object>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<object>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var policies = await _keycloakService.GetAllPoliciesAsync(request.RealmName, request.ClientId, adminToken, cancellationToken);
        return Result<object>.Success(policies, _localizer["OperationSuccess"]);
    }
}
