using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Policy.Delete;

public class DeletePolicyCommand : IRequest<Result<bool>>
{
    public Guid PolicyId { get; set; }
    public string RealmName { get; set; } = string.Empty;
    public string KeycloakClientId { get; set; } = string.Empty;
    public string KeycloakPolicyId { get; set; } = string.Empty;
}
