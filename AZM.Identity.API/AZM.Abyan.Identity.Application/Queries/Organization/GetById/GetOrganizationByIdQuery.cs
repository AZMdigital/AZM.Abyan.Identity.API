using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetById;

public class GetOrganizationByIdQuery(string realmName, string organizationId) : IRequest<Result<OrganizationResponse>>
{
    public string RealmName { get; set; } = realmName;
    public string OrganizationId { get; set; } = organizationId;
}
