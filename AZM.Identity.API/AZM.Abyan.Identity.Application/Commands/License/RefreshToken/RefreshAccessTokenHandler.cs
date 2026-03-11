using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AZM.Abyan.Identity.Application.Commands.License.RefreshToken;

public class RefreshAccessTokenHandler(
    TokenService tokenService,
    ILogger<RefreshAccessTokenHandler> logger)
    : IRequestHandler<RefreshAccessTokenCommand, RefreshAccessTokenResponse>
{
    public async Task<RefreshAccessTokenResponse> Handle(
        RefreshAccessTokenCommand request, CancellationToken ct)
    {
        try
        {
            var response = await tokenService.RefreshAccessTokenAsync(request.RefreshToken, ct);
            return response;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            throw;
        }
    }
}