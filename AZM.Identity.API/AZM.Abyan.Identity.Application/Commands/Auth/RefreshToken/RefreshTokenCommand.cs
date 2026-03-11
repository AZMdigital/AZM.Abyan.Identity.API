using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Auth.RefreshToken;

public class RefreshTokenCommand(RefreshTokenRequest request) : IRequest<Result<LoginResponse>>
{
    public string RefreshToken { get; set; } = request.RefreshToken;
}
