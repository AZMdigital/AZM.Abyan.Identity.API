using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Services;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Auth.RefreshToken;

public class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IAuthService _authService = authService;

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = request.RefreshToken };
        var response = await _authService.RefreshTokenAsync(refreshTokenRequest, cancellationToken);
        return Result<LoginResponse>.Success(response);
    }
}
