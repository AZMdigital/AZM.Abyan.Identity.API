using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Role.Assign;

public class AssignClientRoleToUserCommand : IRequest<Result<bool>>
{
    public AssignRoleRequest AssignRoleRequest { get; set; } = null!;
    public string Realm { get; set; } = string.Empty;
}
