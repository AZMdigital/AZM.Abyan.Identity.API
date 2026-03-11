using AZM.Abyan.Identity.Application.DTOs.Policies;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Policy.GetPolicies;

public class GetPoliciesQuery(string realmName, string clientId) : IRequest<Result<object>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientId { get; set; } = clientId;
}
