using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Update;

public class UpdateOrganizationCommand(Guid id, UpdateOrganizationRequest updateRequest, string realmName) : IRequest<Result<bool>>
{
    public Guid Id { get; set; } = id;
    public UpdateOrganizationRequest UpdateOrganizationRequest { get; set; } = updateRequest;
    public string RealmName { get; set; } = realmName;
}
