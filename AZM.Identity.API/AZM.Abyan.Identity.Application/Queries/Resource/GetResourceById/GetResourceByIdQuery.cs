using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Resource.GetResourceById;

public class GetResourceByIdQuery(string realmName, string clientId, Guid resourceId) : IRequest<Result<object>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientId { get; set; } = clientId;
    public Guid ResourceId { get; set; } = resourceId;
}
