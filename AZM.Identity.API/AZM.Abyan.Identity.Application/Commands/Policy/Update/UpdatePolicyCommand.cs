using AZM.Abyan.Identity.Application.DTOs.Policies;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Policy.Update;

public class UpdatePolicyCommand : IRequest<Result<bool>>
{
    public Guid PolicyId { get; set; }
    public UpdatePolicyRequest UpdatePolicyRequest { get; set; } = null!;
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
    public string KeycloakPolicyId { get; set; } = string.Empty;
}
