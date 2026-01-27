using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.Commands.Role.Unassign;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Unassign
{
    public class RemoveClientPermissionFromUserCommandHandler(
    IRepository<Domain.Entities.TenantUserPermission, Guid> tenantUserPermissionRepository,
    IRepository<Domain.Entities.Permission, Guid> permissionRepository,
    IKeycloakService keycloakService,
    IRealmResolverService realmResolverService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<RemoveClientPermissionFromUserCommand, Result<bool>>
    {
        private readonly IRepository<Domain.Entities.TenantUserPermission, Guid> _tenantUserPermissionRepository = tenantUserPermissionRepository;
        private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;
        private readonly IKeycloakService _keycloakService = keycloakService;
        private readonly IRealmResolverService _realmResolverService = realmResolverService;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;

        public async Task<Result<bool>> Handle(RemoveClientPermissionFromUserCommand request, CancellationToken cancellationToken)
        {
            try
            {  // Resolve TenantId from Realm
                var tenantId = await _realmResolverService.ResolveRealmIdAsync(request.Realm, cancellationToken);
                if (!tenantId.HasValue)
                {
                    return Result<bool>.Failure(_localizer["TenantNotFound"] ?? $"Tenant/Realm '{request.Realm}' not found");
                }

                // Parse UserId
                if (!Guid.TryParse(request.AssignPermissionRequest.UserId, out var userId))
                {
                    return Result<bool>.Failure(_localizer["InvalidUserId"] ?? "Invalid user ID format");
                }

                // Parse ClientId to Guid
                if (!Guid.TryParse(request.AssignPermissionRequest.ClientId, out var clientIdGuid))
                {
                    return Result<bool>.Failure(_localizer["InvalidClientId"] ?? "Invalid client ID format");
                }
                // Get permission by name and clientId from database
                var permission = await _permissionRepository
                    .GetWhere(r => r.Name == request.AssignPermissionRequest.PermissionName)
                    .FirstOrDefaultAsync(cancellationToken);

                if (permission == null)
                {
                    return Result<bool>.NotFound(_localizer["PermissionNotFound"] ?? $"permission '{request.AssignPermissionRequest.PermissionName}' not found for client '{request.AssignPermissionRequest.ClientId}'");
                }

                // Find existing assignment
                var existingAssignment = await _tenantUserPermissionRepository
                    .GetWhere(tur => tur.UserId == userId && tur.PermissionId == permission.Id && tur.TenantId == tenantId.Value)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAssignment == null)
                {
                    return Result<bool>.NotFound(_localizer["PermissionAssignmentNotFound"] ?? "Permission assignment not found");
                }
                // Remove role from Keycloak first
                var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
                await _keycloakService.RemoveClientRoleFromUserAsync(
                    request.Realm,
                    request.AssignPermissionRequest.UserId,
                    request.AssignPermissionRequest.ClientId,
                    request.AssignPermissionRequest.PermissionName,
                    adminToken,
                    cancellationToken);

                // Soft delete assignment from TenantUserRole table
                existingAssignment.SoftDelete();
                _tenantUserPermissionRepository.Update(existingAssignment);
                await _tenantUserPermissionRepository.SaveChangesAsync(cancellationToken);
                return Result<bool>.Deleted(true, _localizer["PermissionRemovedSuccessfully"] ?? "Permission removed successfully");  
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }
    }
}
