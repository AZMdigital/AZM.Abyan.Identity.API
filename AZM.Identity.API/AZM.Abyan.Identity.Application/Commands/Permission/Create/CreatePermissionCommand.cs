using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Create;

public class CreatePermissionCommand : BaseEntity, IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Role-based permission model
    public required string Controller { get; set; } // Mandatory: Controller name
    public string? Action { get; set; } // Optional: Action name

    public Guid ClientId { get; set; } // Local client ID (Guid, same as Keycloak ID)
    public string RealmName { get; set; } = string.Empty;
    // public string KeycloakClientId { get; set; } = string.Empty; // Keycloak client ID (string)
}

