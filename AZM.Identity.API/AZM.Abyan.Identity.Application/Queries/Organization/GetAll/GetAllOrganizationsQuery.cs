using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetAll;

public class GetAllOrganizationsQuery : IRequest<Result<List<OrganizationResponse>>>
{
    public string RealmName { get; set; } = string.Empty;
    public string? Search { get; set; }
}
