using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Role.Update;

public class UpdateRoleCommand : IRequest<Result<bool>>
{
    public Guid RoleId { get; set; }
    public UpdateClientRoleRequest UpdateRoleRequest { get; set; } = null!;
    public string Realm { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty; // Original role name for Keycloak update
}
