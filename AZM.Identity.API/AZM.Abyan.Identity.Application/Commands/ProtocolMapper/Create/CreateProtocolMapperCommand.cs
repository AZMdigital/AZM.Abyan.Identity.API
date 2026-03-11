using AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Create;

public class CreateProtocolMapperCommand(string realmName, string clientScopeName, string clientId, CreateProtocolMapperRequest request) : IRequest<Result<ProtocolMapperResponse>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientScopeName { get; set; } = clientScopeName;
    public string ClientId { get; set; } = clientId;
    public CreateProtocolMapperRequest Request { get; set; } = request;
}
