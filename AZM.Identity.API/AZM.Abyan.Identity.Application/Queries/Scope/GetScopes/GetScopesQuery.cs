using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Scopes;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Scope.GetScopes;

public class GetScopesQuery(string realmName, string clientId) : IRequest<Result<object>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientId { get; set; } = clientId;
}
