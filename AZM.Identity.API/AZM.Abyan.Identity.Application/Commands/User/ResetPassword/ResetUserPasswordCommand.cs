using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.User.ResetPassword;

public class ResetUserPasswordCommand(string userId, ResetPasswordRequest request) : IRequest<Result<bool>>
{
    public string UserId { get; set; } = userId;
    public ResetPasswordRequest Request { get; set; } = request;
}
