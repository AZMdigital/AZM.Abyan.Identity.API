using AZM.Abyan.Identity.Application.DTOs.Permissions;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Update;

public class UpdatePermissionCommand : IRequest<Result<bool>>
{
    public Guid PermissionId { get; set; }
    public UpdatePermissionRequest UpdatePermissionRequest { get; set; } = null!;
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty; // Keycloak client ID (string)
}

