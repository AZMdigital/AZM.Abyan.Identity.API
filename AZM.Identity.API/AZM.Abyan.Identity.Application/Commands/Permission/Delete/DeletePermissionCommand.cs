using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Delete;

public class DeletePermissionCommand : IRequest<Result<bool>>
{
    public Guid PermissionId { get; set; }
}

