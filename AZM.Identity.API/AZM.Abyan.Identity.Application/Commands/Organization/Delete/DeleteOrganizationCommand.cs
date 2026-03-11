using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Delete;

public class DeleteOrganizationCommand(Guid id, string realmName) : IRequest<Result<bool>>
{
    public Guid Id { get; set; } = id;
    public string RealmName { get; set; } = realmName;
}
