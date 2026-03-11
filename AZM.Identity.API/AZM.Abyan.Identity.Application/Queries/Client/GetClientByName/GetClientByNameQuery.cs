using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClientByName;

public class GetClientByNameQuery(string realmName, string clientName) : IRequest<Result<ClientResponse>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientName { get; set; } = clientName;
}
