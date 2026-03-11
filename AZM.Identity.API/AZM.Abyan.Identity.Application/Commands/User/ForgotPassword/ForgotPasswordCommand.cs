using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.User.ForgotPassword;

public class ForgotPasswordCommand(ForgotPasswordRequest request) : IRequest<Result<bool>>
{
    public ForgotPasswordRequest Request { get; set; } = request;
}
