using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Client.Create;

public class CreateClientCommand : BaseEntity, IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = [];
    public string RealmName { get; set; } = string.Empty;
}
