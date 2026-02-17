using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Create;

public class CreateOrganizationCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Alias { get; set; }
    public List<string> Domains { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public string RealmName { get; set; } = string.Empty;
}
