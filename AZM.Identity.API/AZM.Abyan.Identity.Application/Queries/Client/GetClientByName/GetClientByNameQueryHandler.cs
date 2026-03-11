using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Queries.Client.GetClientById;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClientByName;

public class GetClientByNameQueryHandler(IClientService clientService, IMediator mediator, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetClientByNameQuery, Result<ClientResponse>>
{
    private readonly IClientService _clientService = clientService;
    private readonly IMediator _mediator = mediator;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<ClientResponse>> Handle(GetClientByNameQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetClientByIdAsync(request.RealmName, request.ClientName, cancellationToken);
        
        if (client == null)
            return Result<ClientResponse>.NotFound(_localizer["ClientNotFound"]);

        var clientResponse = new ClientResponse
        {
            Name = client.Name,
            Description = client.Description,
            ClientId = client.ClientId,
            RedirectUris = client.RedirectUris
        };

        var getId = await _mediator.Send(new GetClientByKeycloakIdQuery(client.Id), cancellationToken);
        clientResponse.Id = getId.Data;

        return Result<ClientResponse>.Success(clientResponse);
    }
}
