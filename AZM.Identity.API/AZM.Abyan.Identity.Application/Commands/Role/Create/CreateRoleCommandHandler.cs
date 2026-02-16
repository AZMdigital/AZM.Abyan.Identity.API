using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Role.Create;

public class CreateRoleCommandHandler(
    IRepository<Domain.Entities.Role, Guid> roleRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.Role, Guid> _roleRepository = roleRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Create role in Keycloak first
            var createRoleRequest = new CreateClientRoleRequest
            {
                Name = request.Name,
                Description = request.Description
            };

            await _keycloakService.CreateClientRoleAsync(request.Realm, request.KeycloakClientId, createRoleRequest, adminToken, cancellationToken);

            // Get the created role from Keycloak to get its ID
            var keycloakRoles = await _keycloakService.GetClientRolesAsync(request.Realm, request.KeycloakClientId, adminToken, cancellationToken);
            var createdRole = keycloakRoles.FirstOrDefault(r => r.Name == request.Name);

            if (createdRole == null || string.IsNullOrEmpty(createdRole.Id) || !Guid.TryParse(createdRole.Id, out var keycloakRoleId))
            {
                return Result<Guid>.Failure(_localizer["FailedToGetRoleIdFromKeycloak"] ?? "Failed to get role ID from Keycloak");
            }

            // Create local entity with ID from Keycloak
            // Note: ClientId is now the same as KeycloakClientId (both are Keycloak IDs)
            var role = new Domain.Entities.Role
            {
                Id = keycloakRoleId,
                Name = request.Name,
                Description = request.Description,
                ClientId = request.ClientId, // This is the Keycloak client ID (Guid)
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _roleRepository.CreateAsync(role, cancellationToken);
            await _roleRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(role.Id, _localizer["RoleCreatedSuccessfully"] ?? "Role created successfully");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

