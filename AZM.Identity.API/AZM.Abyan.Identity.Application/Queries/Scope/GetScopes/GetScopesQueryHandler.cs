using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Scope.GetScopes;

public class GetScopesQueryHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetScopesQuery, Result<object>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<object>> Handle(GetScopesQuery request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var scopes = await _keycloakService.GetAllScopesAsync(request.RealmName, request.ClientId, adminToken, cancellationToken);
        return Result<object>.Success(scopes, _localizer["OperationSuccess"]);
    }
}
