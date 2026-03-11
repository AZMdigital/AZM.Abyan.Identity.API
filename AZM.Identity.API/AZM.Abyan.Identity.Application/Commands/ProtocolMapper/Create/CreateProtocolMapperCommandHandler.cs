using AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Create;

public class CreateProtocolMapperCommandHandler(IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateProtocolMapperCommand, Result<ProtocolMapperResponse>>
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<ProtocolMapperResponse>> Handle(CreateProtocolMapperCommand request, CancellationToken cancellationToken)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var mapper = await _keycloakService.CreateProtocolMapperAsync(request.RealmName, request.ClientId, request.ClientScopeName, request.Request, adminToken, cancellationToken);
        return Result<ProtocolMapperResponse>.Success(mapper, _localizer["OperationSuccess"]);
    }
}
