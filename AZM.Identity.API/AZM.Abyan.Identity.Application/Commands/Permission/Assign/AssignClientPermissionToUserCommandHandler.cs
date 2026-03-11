using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.Commands.Role.Assign;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Assign
{
    internal class AssignClientPermissionToUserCommandHandler (
    IRepository<TenantUserPermission, Guid> tenantUserPermissionRepository,
    IRepository<Domain.Entities.User, Guid> userRepository,
    IRepository<Domain.Entities.Permission, Guid> PermissionsRepository,
    IRepository<Tenant, Guid> tenantRepository,
    IKeycloakService keycloakService,
    IRealmResolverService realmResolverService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<AssignClientPermissionToUserCommand, Result<bool>>
    {
        private readonly IRepository<TenantUserPermission, Guid> _tenantUserPermissionRepository = tenantUserPermissionRepository;
        private readonly IRepository<Domain.Entities.User, Guid> _userRepository = userRepository;
        private readonly IRepository<Domain.Entities.Permission, Guid> _permissionsRepository = PermissionsRepository;
        private readonly IRepository<Tenant, Guid> _tenantRepository = tenantRepository;
        private readonly IKeycloakService _keycloakService = keycloakService;
        private readonly IRealmResolverService _realmResolverService = realmResolverService;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        public async Task<Result<bool>> Handle(AssignClientPermissionToUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Resolve TenantId from Realm
                var tenantId = await _realmResolverService.ResolveRealmIdAsync(request.Realm, cancellationToken);
                if (!tenantId.HasValue)
                {
                    return Result<bool>.Failure(_localizer["TenantNotFound"]);
                }

                // Parse UserId
                if (!Guid.TryParse(request.AssignPermissionRequest.UserId, out var userId))
                {
                    return Result<bool>.Failure(_localizer["InvalidUserId"]);
                }

                // Verify user exists
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return Result<bool>.NotFound(_localizer["UserNotFound"]);
                }

                // Parse ClientId to Guid
                if (!Guid.TryParse(request.AssignPermissionRequest.ClientId, out var clientIdGuid))
                {
                    return Result<bool>.Failure(_localizer["InvalidClientId"]);
                }
                var Permissions = await _permissionsRepository
               .GetWhere(r => r.Name == request.AssignPermissionRequest.PermissionName && r.IsDeleted == false)
               .FirstOrDefaultAsync(cancellationToken);
                if (Permissions == null)
                {
                    return Result<bool>.NotFound(_localizer["PermissionNotFound"]);
                }
                // Check if assignment already exists
                var existingAssignment = await _tenantUserPermissionRepository
                .GetWhere(tur => tur.UserId == userId && tur.PermissionId == Permissions.Id && tur.TenantId == tenantId.Value && tur.IsDeleted==false)
                .FirstOrDefaultAsync(cancellationToken);

                if (existingAssignment != null)
                {
                    return Result<bool>.Conflict(_localizer["PermissionUpdatedSuccessfully"]);
                }
                var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
                await _keycloakService.AssignClientRoleToUserAsync(
                    request.Realm,
                    request.AssignPermissionRequest.UserId,
                    request.AssignPermissionRequest.ClientId,
                    request.AssignPermissionRequest.PermissionName,
                    adminToken,
                    cancellationToken);
                // Save assignment to TenantUserPermission table
                var tenantUserPermission = new TenantUserPermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId.Value,
                    UserId = userId,
                    PermissionId = Permissions.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                };
                await _tenantUserPermissionRepository.CreateAsync(tenantUserPermission, cancellationToken);
                await _tenantUserPermissionRepository.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true, _localizer["PermissionAssignedSuccessfully"]);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }
    }
}
