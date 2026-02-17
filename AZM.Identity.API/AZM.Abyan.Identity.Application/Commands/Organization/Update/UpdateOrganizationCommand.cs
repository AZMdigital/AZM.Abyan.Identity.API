using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Update;

public class UpdateOrganizationCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public UpdateOrganizationRequest UpdateOrganizationRequest { get; set; } = new();
    public string RealmName { get; set; } = string.Empty;

    public UpdateOrganizationCommand() { }

    public UpdateOrganizationCommand(Guid id, UpdateOrganizationRequest updateRequest, string realmName)
    {
        Id = id;
        UpdateOrganizationRequest = updateRequest;
        RealmName = realmName;
    }
}
