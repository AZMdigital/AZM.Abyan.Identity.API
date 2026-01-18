using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Role.Update;

public class UpdateRoleCommandHandler(
    IRepository<Domain.Entities.Role, Guid> roleRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateRoleCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Role, Guid> _roleRepository = roleRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role == null)
            {
                return Result<bool>.NotFound(_localizer["RoleNotFound"] ?? "Role not found");
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Update role in Keycloak first
            await _keycloakService.UpdateClientRoleAsync(
                request.Realm,
                request.KeycloakClientId,
                request.RoleName, // Use original role name for Keycloak update
                request.UpdateRoleRequest,
                adminToken,
                cancellationToken);

            // Update role in database
            role.Name = request.UpdateRoleRequest.Name;
            role.Description = request.UpdateRoleRequest.Description;
            role.UpdatedAt = DateTime.UtcNow;
            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Updated(true, _localizer["RoleUpdatedSuccessfully"] ?? "Role updated successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
