using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Role.Delete;

public class DeleteRoleCommandHandler(
    IRepository<Domain.Entities.Role, Guid> roleRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Role, Guid> _roleRepository = roleRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role == null)
            {
                return Result<bool>.NotFound(_localizer["RoleNotFound"]);
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Delete role in Keycloak first
            await _keycloakService.DeleteClientRoleAsync(
                request.Realm,
                request.KeycloakClientId,
                request.RoleName,
                adminToken,
                cancellationToken);

            // Soft delete role in database
            role.SoftDelete();
            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Deleted(true, _localizer["RoleDeletedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
