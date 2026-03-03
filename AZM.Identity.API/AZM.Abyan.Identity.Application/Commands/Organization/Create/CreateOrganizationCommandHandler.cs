using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Organization.Create;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Result<Guid>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly IRealmResolverService _realmResolverService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateOrganizationCommandHandler(
        ITenantRepository tenantRepository,
        IKeycloakService keycloakService,
        IRealmResolverService realmResolverService,
        IStringLocalizer<SharedResource> localizer)
    {
        _tenantRepository = tenantRepository;
        _keycloakService = keycloakService;
        _realmResolverService = realmResolverService;
        _localizer = localizer;
    }

    public async Task<Result<Guid>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Create organization in Keycloak
            var createOrgRequest = new CreateOrganizationRequest
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Alias = request.Alias ?? request.Name,
                Domains = request.Domains,
                Enabled = request.Enabled
            };

            var keycloakOrgId = await _keycloakService.CreateOrganizationAsync(
                request.RealmName,
                createOrgRequest,
                adminToken,
                cancellationToken);

            // Create Tenant entity in database
            var tenant = new Tenant
            {
                Id = Guid.Parse(keycloakOrgId),
                Name = request.Name,
                IsActive = request.Enabled,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _tenantRepository.AddAsync(tenant, cancellationToken);

            return Result<Guid>.Created(tenant.Id, _localizer["OrganizationCreatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
