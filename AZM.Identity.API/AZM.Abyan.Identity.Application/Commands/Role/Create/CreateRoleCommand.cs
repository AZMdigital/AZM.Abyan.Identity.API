using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Role.Create;

public class CreateRoleCommand : BaseEntity, IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string Realm { get; set; } = string.Empty;
  //  public Guid KeycloakClientId { get; set; } = string.Empty; // Keycloak client ID (string)
}

