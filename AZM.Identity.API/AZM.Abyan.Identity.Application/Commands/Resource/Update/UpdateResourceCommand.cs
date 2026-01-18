using AZM.Abyan.Identity.Application.DTOs.Resources;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Resource.Update;

public class UpdateResourceCommand : IRequest<Result<bool>>
{
    public Guid ResourceId { get; set; }
    public UpdateResourceRequest UpdateResourceRequest { get; set; } = null!;
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
}
