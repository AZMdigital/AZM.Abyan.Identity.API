using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Scope.GetScopeByName;

public class GetScopeByNameQueryHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetScopeByNameQuery, Result<object>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<object>> Handle(GetScopeByNameQuery request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var scope = await _keycloakService.GetScopeAsync(request.RealmName, request.ClientId, request.ScopeName, adminToken, cancellationToken);

        if (scope == null)
            return Result<object>.NotFound(_localizer["ScopeNotFound"]);

        return Result<object>.Success(scope, _localizer["OperationSuccess"]);
    }
}
