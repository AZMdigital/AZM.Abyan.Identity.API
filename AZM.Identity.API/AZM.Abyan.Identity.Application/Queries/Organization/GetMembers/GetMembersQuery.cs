using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetMembers;

public class GetMembersQuery(string realmName, string organizationId) : IRequest<Result<List<UserResponse>>>
{
    public string RealmName { get; set; } = realmName;
    public string OrganizationId { get; set; } = organizationId;
}
