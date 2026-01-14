using AZM.Abyan.Identity.Application.DTOs.Policies;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Policy.Create;

public class CreatePolicyCommand : IRequest<Result<Guid>>
{
    public CreatePolicyRequest CreatePolicyRequest { get; set; } = null!;
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
}
