using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Queries.Client.GetClientById;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClients;

public class GetClientsQueryHandler(IClientService clientService, IMediator mediator, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetClientsQuery, Result<List<ClientResponse>>>
{
    private readonly IClientService _clientService = clientService;
    private readonly IMediator _mediator = mediator;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<List<ClientResponse>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await _clientService.GetClientsAsync(request.RealmName, cancellationToken);
        var response = new List<ClientResponse>();

        if (clients.Count > 0)
        {
            foreach (var client in clients)
            {
                var clientResponse = new ClientResponse
                {
                    Name = client.Name,
                    Description = client.Description,
                    ClientId = client.ClientId,
                    RedirectUris = client.RedirectUris
                };

                var getId = await _mediator.Send(new GetClientByKeycloakIdQuery(client.Id), cancellationToken);
                clientResponse.Id = getId.Data;

                response.Add(clientResponse);
            }
        }

        return Result<List<ClientResponse>>.Success(response);
    }
}
