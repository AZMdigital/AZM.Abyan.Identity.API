using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Disable;

public class DisableProtocolMapperCommand(string realmName, string clientScopeId, string mapperId) : IRequest<Result<bool>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientScopeId { get; set; } = clientScopeId;
    public string MapperId { get; set; } = mapperId;
}
