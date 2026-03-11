using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Role.GetClientRoles;

public class GetClientRolesQueryHandler(IRoleService roleService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetClientRolesQuery, Result<List<ClientRoleResponse>>>
{
    private readonly IRoleService _roleService = roleService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<List<ClientRoleResponse>>> Handle(GetClientRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetClientRolesAsync(request.RealmName, request.ClientId, cancellationToken);
        return Result<List<ClientRoleResponse>>.Success(roles, _localizer["OperationSuccess"]);
    }
}
