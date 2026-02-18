using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Update;

public class UpdatePermissionCommandHandler(
    IRepository<Domain.Entities.Permission, Guid> permissionRepository,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdatePermissionCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result<bool>.NotFound(_localizer["PermissionNotFound"] ?? "Permission not found");
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.UpdatePermissionRequest.Name))
        {
            permission.Name = request.UpdatePermissionRequest.Name;
        }

        if (request.UpdatePermissionRequest.Description != null)
        {
            permission.Description = request.UpdatePermissionRequest.Description;
        }

        if (request.UpdatePermissionRequest.ScopeId.HasValue)
        {
            permission.ScopeId = request.UpdatePermissionRequest.ScopeId.Value;
        }

        if (request.UpdatePermissionRequest.ResourceId.HasValue)
        {
            permission.ResourceId = request.UpdatePermissionRequest.ResourceId.Value;
        }

        if (request.UpdatePermissionRequest.PolicyId.HasValue)
        {
            permission.PolicyId = request.UpdatePermissionRequest.PolicyId.Value;
        }

        permission.UpdatedAt = DateTime.UtcNow;
        permission.UpdatedBy = Guid.Empty;

        _permissionRepository.Update(permission);
        await _permissionRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Updated(true, _localizer["PermissionUpdatedSuccessfully"] ?? "Permission updated successfully");
    }
}

