using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Client.Update
{
    public class UpdateClientCommandHandler(IClientRepository repository, IClientService clientService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateClientCommand, Result<bool>>
    {
        private readonly IClientRepository _repository = repository;
        private readonly IClientService _clientService = clientService;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        public async Task<Result<bool>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            // First update client in Keycloak
            await _clientService.UpdateClientAsync(request.RealmName, request.UpdateClientRequest.ClientId, request.UpdateClientRequest, cancellationToken);

            // ClientId from request is now the Keycloak ID which is the same as entity Id
            var client = await _repository.GetClientByKeycloakIdAsync(Guid.Parse(request.UpdateClientRequest.ClientId));
            if (client == null)
            {
                return Result<bool>.NotFound(_localizer["ClientNotFound"]);
            }
            client.Description = request.UpdateClientRequest.Description;
            client.Name = request.UpdateClientRequest.Name;
            var update = await _repository.UpdateAsync(client, cancellationToken);
            if (!update)
            {
                return Result<bool>.NotFound(_localizer["ClientNotFound"]);
            }

            return Result<bool>.Updated(true, _localizer["ClientUpdateSuccessfully"]);
        }
    }
}
