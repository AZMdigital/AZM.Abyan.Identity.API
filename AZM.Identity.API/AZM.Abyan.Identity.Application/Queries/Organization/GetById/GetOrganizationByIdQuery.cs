using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetById;

public class GetOrganizationByIdQuery : IRequest<Result<OrganizationResponse>>
{
    public string RealmName { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;

    public GetOrganizationByIdQuery() { }

    public GetOrganizationByIdQuery(string realmName, string organizationId)
    {
        RealmName = realmName;
        OrganizationId = organizationId;
    }
}
