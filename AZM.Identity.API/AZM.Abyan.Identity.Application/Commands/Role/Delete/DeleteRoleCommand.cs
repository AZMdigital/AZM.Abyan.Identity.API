using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Role.Delete;

public class DeleteRoleCommand : IRequest<Result<bool>>
{
    public Guid RoleId { get; set; }
    public string Realm { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty; // Role name for Keycloak delete
}
