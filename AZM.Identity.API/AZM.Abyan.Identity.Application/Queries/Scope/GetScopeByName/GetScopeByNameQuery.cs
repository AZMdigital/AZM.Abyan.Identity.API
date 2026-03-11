using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Scope.GetScopeByName;

public class GetScopeByNameQuery(string realmName, string clientId, string scopeName) : IRequest<Result<object>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientId { get; set; } = clientId;
    public string ScopeName { get; set; } = scopeName;
}
