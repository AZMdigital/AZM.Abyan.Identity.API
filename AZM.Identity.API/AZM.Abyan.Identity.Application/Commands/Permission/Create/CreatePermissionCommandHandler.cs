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

            // Get Scope, Resource, and Policy entities to get their names for Keycloak
            var scope = await _scopeRepository.GetByIdAsync(request.ScopeId, cancellationToken);
            if (scope == null)
            {
                return Result<Guid>.Failure(_localizer["ScopeNotFound"] ?? $"Scope with ID {request.ScopeId} not found");
            }

            var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
            if (resource == null)
            {
                return Result<Guid>.Failure(_localizer["ResourceNotFound"] ?? $"Resource with ID {request.ResourceId} not found");
            }

            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
            if (policy == null)
            {
                return Result<Guid>.Failure(_localizer["PolicyNotFound"] ?? $"Policy with ID {request.PolicyId} not found");
            }

            // Create permission in Keycloak first
            // Keycloak needs resource names, scope names, and policy names
            var keycloakPermissionId = await _keycloakService.CreateScopePermissionAsync(
                request.RealmName,
                request.KeycloakClientId,
                request.Name,
                new[] { resource.Name },
                new[] { scope.Name },
                new[] { policy.Name },
                adminToken,
                cancellationToken);

            if (string.IsNullOrEmpty(keycloakPermissionId) || !Guid.TryParse(keycloakPermissionId, out var keycloakPermissionIdGuid))
            {
                return Result<Guid>.Failure(_localizer["FailedToCreatePermissionInKeycloak"] ?? "Failed to create permission in Keycloak or retrieve its ID");
            }

            // Create local entity with ID from Keycloak
            var permission = new Domain.Entities.Permission
            {
                Id = keycloakPermissionIdGuid,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                ScopeId = request.ScopeId,
                ResourceId = request.ResourceId,
                PolicyId = request.PolicyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _permissionRepository.CreateAsync(permission, cancellationToken);
            await _permissionRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(permission.Id, _localizer["PermissionCreatedSuccessfully"] ?? "Permission created successfully");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

