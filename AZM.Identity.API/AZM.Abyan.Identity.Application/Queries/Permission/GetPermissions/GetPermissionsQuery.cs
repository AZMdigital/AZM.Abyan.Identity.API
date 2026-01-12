using AZM.Abyan.Identity.Application.DTOs.Permissions;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Permission.GetPermissions;

public class GetPermissionsQuery : IRequest<Result<List<PermissionResponse>>>
{
    public Guid? ClientId { get; set; } // Optional: filter by client if needed
}

