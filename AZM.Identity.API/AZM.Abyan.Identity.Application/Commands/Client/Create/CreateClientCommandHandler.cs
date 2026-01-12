using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Domain.Entities;
namespace AZM.Abyan.Identity.Application.Commands.Client.Create
{
    public class CreateClientCommandHandler(IClientRepository clientRepository, IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateClientCommand, Result<Guid>>
    {
        private readonly IClientRepository _repository = clientRepository;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
            var client = request.Adapt<Domain.Entities.Client>();
            client.Id =Guid.NewGuid(); 
            client.Name = request.Name;
            client.Description = request.Description;
            client.RealmId = request.RealmId;
            client.KeycloakClientId = request.KeycloakClientId;
            client.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(client,cancellationToken);

            return Result<Guid>.Created(client.Id, _localizer["ClientCreatedSuccessfully"]);
            }catch(Exception ex)
            {
                return Result<Guid>.Created(Guid.Empty,ex.Message); ;
            }
        }
    }
}
