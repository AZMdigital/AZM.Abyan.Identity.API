using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Policy.GetPolicyByName;

public class GetPolicyByNameQuery(string realmName, string clientId, string policyName) : IRequest<Result<object>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientId { get; set; } = clientId;
    public string PolicyName { get; set; } = policyName;
}
