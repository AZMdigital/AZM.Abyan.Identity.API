using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.User.Delete;

public class DeleteUserCommand(string userId) : IRequest<Result<bool>>
{
    public string UserId { get; set; } = userId;
}
