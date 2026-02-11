using AZM.Abyan.Identity.Application.DTOs.Auth;

namespace AZM.Abyan.Identity.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    //Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutUserAsync(string userId, CancellationToken cancellationToken = default);
}

