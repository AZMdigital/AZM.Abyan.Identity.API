using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Delete;

public class DeletePermissionCommandHandler(
    IRepository<Domain.Entities.Permission, Guid> permissionRepository,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeletePermissionCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result<bool>.NotFound(_localizer["PermissionNotFound"] ?? "Permission not found");
        }

        // Soft delete
        permission.SoftDelete();
        _permissionRepository.Update(permission);
        await _permissionRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Deleted(true, _localizer["PermissionDeletedSuccessfully"] ?? "Permission deleted successfully");
    }
}

