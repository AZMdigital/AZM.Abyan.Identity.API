using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Delete;

public class DeleteOrganizationCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public string RealmName { get; set; } = string.Empty;

    public DeleteOrganizationCommand() { }

    public DeleteOrganizationCommand(Guid id, string realmName)
    {
        Id = id;
        RealmName = realmName;
    }
}
