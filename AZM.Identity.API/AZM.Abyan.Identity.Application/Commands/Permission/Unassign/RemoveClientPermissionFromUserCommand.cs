using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Unassign;

public class RemoveClientPermissionFromUserCommand : IRequest<Result<bool>>
{
    public AssignPermissionRequest AssignPermissionRequest { get; set; } = null!;
    public string Realm { get; set; } = string.Empty;
}

