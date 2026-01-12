using AZM.Abyan.Identity.Application.DTOs.Permissions;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Permission.GetPermissionById;

public class GetPermissionByIdQuery : IRequest<Result<PermissionResponse>>
{
    public Guid PermissionId { get; set; }
}

