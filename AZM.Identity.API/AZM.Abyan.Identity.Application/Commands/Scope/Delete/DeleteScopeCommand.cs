using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Scope.Delete;

public class DeleteScopeCommand : IRequest<Result<bool>>
{
    public Guid ScopeId { get; set; }
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
    public string KeycloakScopeId { get; set; } = string.Empty;
}
