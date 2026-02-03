using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
namespace AZM.Abyan.Identity.Application.Commands.Client.Create
{
    public class CreateClientCommandHandler(
        IClientRepository clientRepository,
        IKeycloakService keycloakService,
        IRealmResolverService realmResolverService,
        IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateClientCommand, Result<Guid>>
    {
        private readonly IClientRepository _repository = clientRepository;
        private readonly IKeycloakService _keycloakService = keycloakService;
        private readonly IRealmResolverService _realmResolverService = realmResolverService;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        
        public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Resolve realm ID from realm name
                var realmId = await _realmResolverService.ResolveRealmIdAsync(request.RealmName, cancellationToken);
                if (!realmId.HasValue)
                {
                    return Result<Guid>.Failure(_localizer["TenantNotFound"] ?? $"Tenant/Realm '{request.RealmName}' not found");
                }

                // Get admin token
                var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

                // Create client in Keycloak first
                var createClientRequest = new CreateClientRequest
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    RedirectUris=request.RedirectUris
                };

                var keycloakClientId = await _keycloakService.CreateClientAsync(request.RealmName, createClientRequest, adminToken, cancellationToken);

                // Create local entity with ID from Keycloak
                var client = new Domain.Entities.Client
                {
                    Id = keycloakClientId,
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    RealmId = realmId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                };

                await _repository.AddAsync(client, cancellationToken);

                return Result<Guid>.Created(client.Id, _localizer["ClientCreatedSuccessfully"]);
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure(ex.Message);
            }
        }
    }
}
