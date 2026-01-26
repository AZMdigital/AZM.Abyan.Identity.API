using AZM.Abyan.Identity.Application.DTOs.Permissions;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Application.Queries.Permission.GetPermissions;

public class GetPermissionsQueryHandler(
    IRepository<Domain.Entities.Permission, Guid> permissionRepository) : IRequestHandler<GetPermissionsQuery, Result<List<PermissionResponse>>>
{
    private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;

    public async Task<Result<List<PermissionResponse>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _permissionRepository.GetWhere(p => !p.IsDeleted);

        var permissions = await query.ToListAsync(cancellationToken);

        var response = permissions.Select(p => new PermissionResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Controller = p.Controller,
            Action = p.Action,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return Result<List<PermissionResponse>>.Success(response);
    }
}

