using AZM.Abyan.Identity.Application.DTOs.Scopes;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Scope.Update;

public class UpdateScopeCommand : IRequest<Result<bool>>
{
    public Guid ScopeId { get; set; }
    public UpdateScopeRequest UpdateScopeRequest { get; set; } = null!;
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
    public string KeycloakScopeId { get; set; } = string.Empty; // Keycloak scope ID (string)
}
