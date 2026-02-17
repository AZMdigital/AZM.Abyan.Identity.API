using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Create;

public class CreatePermissionCommandHandler(
    IRepository<Domain.Entities.Permission, Guid> permissionRepository,
    IRepository<Domain.Entities.Scope, Guid> scopeRepository,
    IRepository<Domain.Entities.Resource, Guid> resourceRepository,
    IRepository<Domain.Entities.Policy, Guid> policyRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreatePermissionCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;
    private readonly IRepository<Domain.Entities.Scope, Guid> _scopeRepository = scopeRepository;
    private readonly IRepository<Domain.Entities.Resource, Guid> _resourceRepository = resourceRepository;
    private readonly IRepository<Domain.Entities.Policy, Guid> _policyRepository = policyRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Resolve client internal ID from configuration
            //var clientInternalId = ResolveClientInternalId(request.RealmName, request.KeycloakClientId);
            //if (string.IsNullOrEmpty(clientInternalId))
            //{
            //    return Result<Guid>.Failure(_localizer["ClientNotFound"] ?? $"Client '{request.KeycloakClientId}' not found in configuration for realm '{request.RealmName}'");
            //}

            // Parse ClientInternalId to Guid for local ClientId
            //if (!Guid.TryParse(request.ClientId, out var clientIdGuid))
            //{
            //    return Result<Guid>.Failure(_localizer["InvalidClientInternalId"] ?? $"Invalid client internal ID format: {clientInternalId}");
            //}

            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
            if (policy == null)
            {
                return Result<Guid>.Failure(_localizer["PolicyNotFound"] ?? $"Policy with ID {request.PolicyId} not found");
            }

            // Log the IDs we're using
            Console.WriteLine($"Creating permission in Keycloak with:");
            Console.WriteLine($"  Resource ID: {resource.Id}");
            Console.WriteLine($"  Scope ID: {scope.Id}");
            Console.WriteLine($"  Policy ID: {policy.Id}");
            Console.WriteLine($"  Resource Name (for reference): {resource.Name}");
            Console.WriteLine($"  Scope Name (for reference): {scope.Name}");
            Console.WriteLine($"  Policy Name (for reference): {policy.Name}");

            // Create permission in Keycloak using IDs from database (which should match Keycloak IDs)
            // Note: The database IDs should already be the Keycloak IDs from the sync process
            var keycloakPermissionId = await _keycloakService.CreateScopePermissionAsync(
                request.RealmName,
                request.ClientId.ToString(),
                createRoleRequest,
                adminToken,
                cancellationToken);

            // Get the created role from Keycloak to get its ID
            var keycloakRoles = await _keycloakService.GetClientRolesAsync(request.RealmName, request.ClientId.ToString(), adminToken, cancellationToken);
            var createdRole = keycloakRoles.FirstOrDefault(r => r.Name == request.Name);

            if (createdRole == null || string.IsNullOrEmpty(createdRole.Id) || !Guid.TryParse(createdRole.Id, out var keycloakRoleId))
            {
                return Result<Guid>.Failure(_localizer["FailedToCreatePermissionInKeycloak"] ?? "Failed to create permission in Keycloak or retrieve its ID");
            }

            // Verify that the returned ID matches what we expect
            Console.WriteLine($"Permission created in Keycloak with ID: {keycloakPermissionIdGuid}");

            // Create local entity with ID from Keycloak
            var permission = new Domain.Entities.Permission
            {
                Id = keycloakPermissionIdGuid,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                ScopeId = request.ScopeId,        // Should match the Keycloak scope ID
                ResourceId = request.ResourceId,  // Should match the Keycloak resource ID
                PolicyId = request.PolicyId,      // Should match the Keycloak policy ID
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _permissionRepository.CreateAsync(permission, cancellationToken);
            await _permissionRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(permission.Id, _localizer["PermissionCreatedSuccessfully"] ?? "Permission created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating permission: {ex.Message}");
            return Result<Guid>.Failure(ex.Message);
        }
    }
}