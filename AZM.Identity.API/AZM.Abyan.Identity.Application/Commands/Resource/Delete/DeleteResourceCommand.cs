using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Resource.Delete;

public class DeleteResourceCommand : IRequest<Result<bool>>
{
    public Guid ResourceId { get; set; }
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
}
