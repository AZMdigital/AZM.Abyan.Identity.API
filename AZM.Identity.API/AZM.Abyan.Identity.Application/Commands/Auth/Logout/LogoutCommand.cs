using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Auth.Logout;

public class LogoutCommand(Guid userId) : IRequest<Result<bool>>
{
    public Guid UserId { get; set; } = userId;
}
