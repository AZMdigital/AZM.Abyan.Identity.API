using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.Commands.Client.Delete;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Client.Update
{
    public class UpdateClientCommandHandler(IClientRepository repository, IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateClientCommand, Result<bool>>
    {
        private readonly IClientRepository _repository = repository;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        public async Task<Result<bool>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            var client = await _repository.GetClientByKeycloakIdAsync(Guid.Parse(request.UpdateClientRequest.ClientId));
            client.Description = request.UpdateClientRequest.Description;
            client.Name = request.UpdateClientRequest.Name;
            var update =  await _repository.UpdateAsync(client, cancellationToken);
            if (!update)
            {
                return Result<bool>.NotFound(_localizer["ClientNotFound"]);
            }

            return Result<bool>.Updated(true, _localizer["ClientUpdateSuccessfully"]);
        }
    }
}
