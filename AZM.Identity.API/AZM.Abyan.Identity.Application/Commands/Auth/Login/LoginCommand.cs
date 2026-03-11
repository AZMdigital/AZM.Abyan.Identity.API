using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Auth.Login;

public class LoginCommand(LoginRequest request) : IRequest<Result<LoginResponse>>
{
    public string Username { get; set; } = request.Username;
    public string Password { get; set; } = request.Password;
}
