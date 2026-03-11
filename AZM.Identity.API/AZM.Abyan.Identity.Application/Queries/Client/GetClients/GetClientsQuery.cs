using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClients;

public class GetClientsQuery(string realmName) : IRequest<Result<List<ClientResponse>>>
{
    public string RealmName { get; set; } = realmName;
}
