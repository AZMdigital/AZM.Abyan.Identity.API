using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Disable;

public class DisableProtocolMapperCommandHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<DisableProtocolMapperCommand, Result<bool>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(DisableProtocolMapperCommand request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DisableProtocolMapperAsync(request.RealmName, request.ClientScopeId, request.MapperId, adminToken, cancellationToken);
        return Result<bool>.Success(true, _localizer["OperationSuccess"]);
    }
}
