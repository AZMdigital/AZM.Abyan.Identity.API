using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Role.Unassign;

public class RemoveClientRoleFromUserCommandHandler(
    IRepository<Domain.Entities.TenantUserRole, Guid> tenantUserRoleRepository,
    IRepository<Domain.Entities.Role, Guid> roleRepository,
    IKeycloakService keycloakService,
    IRealmResolverService realmResolverService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<RemoveClientRoleFromUserCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.TenantUserRole, Guid> _tenantUserRoleRepository = tenantUserRoleRepository;
    private readonly IRepository<Domain.Entities.Role, Guid> _roleRepository = roleRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IRealmResolverService _realmResolverService = realmResolverService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(RemoveClientRoleFromUserCommand request, CancellationToken cancellationToken)
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
            if (!Guid.TryParse(request.AssignRoleRequest.UserId, out var userId))
            {
                return Result<bool>.Failure(_localizer["InvalidUserId"]);
            }

            // Parse ClientId to Guid
            if (!Guid.TryParse(request.AssignRoleRequest.ClientId, out var clientIdGuid))
            {
                return Result<bool>.Failure(_localizer["InvalidClientId"]);
            }
                // Get role by name and clientId from database
                var role = await _roleRepository
                    .GetWhere(r => r.Name == request.AssignRoleRequest.RoleName && r.ClientId == clientIdGuid)
                    .FirstOrDefaultAsync(cancellationToken);

                if (role == null)
                {
                    return Result<bool>.NotFound(_localizer["RoleNotFound"]);
                }

                // Find existing assignment
                var existingAssignment = await _tenantUserRoleRepository
                    .GetWhere(tur => tur.UserId == userId && tur.RoleId == role.Id && tur.TenantId == tenantId.Value)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAssignment == null)
                {
                    return Result<bool>.NotFound(_localizer["RoleAssignmentNotFound"]);
                }

                // Remove role from Keycloak first
                var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
                await _keycloakService.RemoveClientRoleFromUserAsync(
                    request.Realm,
                    request.AssignRoleRequest.UserId,
                    request.AssignRoleRequest.ClientId,
                    request.AssignRoleRequest.RoleName,
                    adminToken,
                    cancellationToken);

                // Soft delete assignment from TenantUserRole table
                existingAssignment.SoftDelete();
                _tenantUserRoleRepository.Update(existingAssignment);
                await _tenantUserRoleRepository.SaveChangesAsync(cancellationToken);

                return Result<bool>.Deleted(true, _localizer["RoleRemovedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
