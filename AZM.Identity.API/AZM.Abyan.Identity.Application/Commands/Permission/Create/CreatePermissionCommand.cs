using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Create;

public class CreatePermissionCommand : BaseEntity, IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ScopeId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid PolicyId { get; set; }
    public Guid ClientId { get; set; } // Local client ID (Guid, same as Keycloak ID)
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty; // Keycloak client ID (string)
}

