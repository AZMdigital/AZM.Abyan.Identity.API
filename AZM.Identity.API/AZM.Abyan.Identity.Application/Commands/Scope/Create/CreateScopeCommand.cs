using AZM.Abyan.Identity.Application.DTOs.Scopes;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Scope.Create;

public class CreateScopeCommand : IRequest<Result<Guid>>
{
    public CreateScopeRequest CreateScopeRequest { get; set; } = null!;
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
}
