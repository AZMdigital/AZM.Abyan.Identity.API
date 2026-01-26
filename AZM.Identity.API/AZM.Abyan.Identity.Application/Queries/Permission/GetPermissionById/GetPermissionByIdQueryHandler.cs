using AZM.Abyan.Identity.Application.DTOs.Permissions;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Permission.GetPermissionById;

public class GetPermissionByIdQueryHandler(
    IRepository<Domain.Entities.Permission, Guid> permissionRepository,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetPermissionByIdQuery, Result<PermissionResponse>>
{
    private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<PermissionResponse>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetWhere(p => p.Id == request.PermissionId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (permission == null)
        {
            return Result<PermissionResponse>.NotFound(_localizer["PermissionNotFound"] ?? "Permission not found");
        }

        var response = new PermissionResponse
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Controller = permission.Controller,
            Action = permission.Action,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt
        };

        return Result<PermissionResponse>.Success(response);
    }
}

