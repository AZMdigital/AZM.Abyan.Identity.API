using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Update;

public class UpdatePermissionCommandHandler(
    IRepository<Domain.Entities.Permission, Guid> permissionRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdatePermissionCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result<bool>.NotFound(_localizer["PermissionNotFound"] ?? "Permission not found");
        }

        var hasChanges = false;

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.UpdatePermissionRequest.Name))
        {
            permission.Name = request.UpdatePermissionRequest.Name;
            hasChanges = true;
        }

        if (request.UpdatePermissionRequest.Description != null)
        {
            permission.Description = request.UpdatePermissionRequest.Description;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(request.UpdatePermissionRequest.Controller))
        {
            permission.Controller = request.UpdatePermissionRequest.Controller;
            hasChanges = true;
        }

        if (request.UpdatePermissionRequest.Action != null)
        {
            permission.Action = request.UpdatePermissionRequest.Action;
            hasChanges = true;
        }

        if (hasChanges)
        {
            // Update role in Keycloak with new attributes
            try
            {
                var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

                // Prepare updated attributes
                var attributes = new Dictionary<string, string[]>
                {
                    ["Controller"] = new[] { permission.Controller }
                };

                if (!string.IsNullOrEmpty(permission.Action))
                {
                    attributes["Action"] = new[] { permission.Action };
                }

                // Update role in Keycloak
                var updateRoleRequest = new UpdateClientRoleRequest
                {
                    Name = permission.Name,
                    Description = permission.Description ?? string.Empty,
                    Attributes = attributes
                };

                // Update role in Keycloak
                await _keycloakService.UpdateClientRoleAsync(
                    request.RealmName,
                    request.KeycloakClientId,
                    permission.Name,
                    updateRoleRequest,
                    adminToken,
                    cancellationToken);

                permission.UpdatedAt = DateTime.UtcNow;
                permission.UpdatedBy = Guid.Empty;

                _permissionRepository.Update(permission);
                await _permissionRepository.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(_localizer["FailedToUpdatePermissionInKeycloak"] ?? $"Failed to update permission in Keycloak: {ex.Message}");
            }
        }

        return Result<bool>.Updated(true, _localizer["PermissionUpdatedSuccessfully"] ?? "Permission updated successfully");
    }
}

