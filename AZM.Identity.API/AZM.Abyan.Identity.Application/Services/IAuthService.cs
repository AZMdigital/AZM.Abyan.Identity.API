using AZM.Identity.Application.DTOs.Auth;

namespace AZM.Identity.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}

