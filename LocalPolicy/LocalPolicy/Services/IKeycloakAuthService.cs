using LocalPolicy.DTOs;

namespace LocalPolicy.Services;

public interface IKeycloakAuthService
{
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}

