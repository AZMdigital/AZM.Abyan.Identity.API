using AZM.Abyan.Identity.Application.DTOs.Resources;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Resource.Create;

public class CreateResourceCommand : IRequest<Result<Guid>>
{
    public CreateResourceRequest CreateResourceRequest { get; set; } = null!;
    public string RealmName { get; set; } = string.Empty;
    public Guid ClientId { get; set; } // Local client ID
    public string KeycloakClientId { get; set; } = string.Empty; // Keycloak client ID (string)
}
