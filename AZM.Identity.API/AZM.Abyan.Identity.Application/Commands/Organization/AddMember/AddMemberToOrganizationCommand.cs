using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Organization.AddMember;

public class AddMemberToOrganizationCommand(string realmName, string organizationId, string userId) : IRequest<Result<bool>>
{
    public string RealmName { get; set; } = realmName;
    public string OrganizationId { get; set; } = organizationId;
    public string UserId { get; set; } = userId;
}
