using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Policy.GetPolicyByName;

public class GetPolicyByNameQueryHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetPolicyByNameQuery, Result<object>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<object>> Handle(GetPolicyByNameQuery request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var policy = await _keycloakService.GetPolicyAsync(request.RealmName, request.ClientId, request.PolicyName, adminToken, cancellationToken);

        if (policy == null)
            return Result<object>.NotFound(_localizer["PolicyNotFound"]);

        return Result<object>.Success(policy, _localizer["OperationSuccess"]);
    }
}
