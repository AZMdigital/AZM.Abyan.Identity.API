using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.User.Delete;

public class DeleteUserCommand(Guid userId) : IRequest<Result<bool>>
{
    public Guid UserId { get; set; } = userId;
}
