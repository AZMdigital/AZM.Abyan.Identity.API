using AZM.Abyan.Identity.Application.DTOs.Auth;

namespace AZM.Abyan.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IKeycloakService _keycloakService;

    public AuthService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await _keycloakService.LoginAsync(request.Username, request.Password, cancellationToken);
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        return await _keycloakService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
    }

    public async Task LogoutUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _keycloakService.LogoutUserAsync(userId, cancellationToken);
    }
}

