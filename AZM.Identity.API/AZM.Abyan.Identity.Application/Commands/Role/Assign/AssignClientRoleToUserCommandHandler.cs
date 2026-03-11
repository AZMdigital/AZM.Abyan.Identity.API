using System.Data;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Role.Assign;

public class AssignClientRoleToUserCommandHandler(
    IRepository<TenantUserRole, Guid> tenantUserRoleRepository,
    IRepository<Domain.Entities.User, Guid> userRepository,
    IRepository<Domain.Entities.Role, Guid> roleRepository,
    IRepository<Tenant, Guid> tenantRepository,
    IKeycloakService keycloakService,
    IRealmResolverService realmResolverService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<AssignClientRoleToUserCommand, Result<bool>>
{
    private readonly IRepository<TenantUserRole, Guid> _tenantUserRoleRepository = tenantUserRoleRepository;
    private readonly IRepository<Domain.Entities.User, Guid> _userRepository = userRepository;
    private readonly IRepository<Domain.Entities.Role, Guid> _roleRepository = roleRepository;
    private readonly IRepository<Tenant, Guid> _tenantRepository = tenantRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IRealmResolverService _realmResolverService = realmResolverService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(AssignClientRoleToUserCommand request, CancellationToken cancellationToken)
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

            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<bool>.NotFound(_localizer["UserNotFound"]);
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
            // Check if assignment already exists
            var existingAssignment = await _tenantUserRoleRepository
            .GetWhere(tur => tur.UserId == userId && tur.RoleId == role.Id && tur.TenantId == tenantId.Value)
            .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignment != null)
            {
                return Result<bool>.Conflict(_localizer["RoleAlreadyAssigned"]);
            }
            // Assign role in Keycloak first
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            await _keycloakService.AssignClientRoleToUserAsync(
                request.Realm,
                request.AssignRoleRequest.UserId,
                request.AssignRoleRequest.ClientId,
                request.AssignRoleRequest.RoleName,
                adminToken,
                cancellationToken);
            // Save assignment to TenantUserRole table
            var tenantUserRole = new TenantUserRole
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                UserId = userId,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _tenantUserRoleRepository.CreateAsync(tenantUserRole, cancellationToken);
            await _tenantUserRoleRepository.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true, _localizer["RoleAssignedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
