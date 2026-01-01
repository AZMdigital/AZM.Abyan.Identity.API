using AZM.Abyan.Identity.Application.DTOs.Users;

namespace AZM.Abyan.Identity.Application.Services;

public class UserService : IUserService
{
    private readonly IKeycloakService _keycloakService;

    public UserService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.CreateUserAsync(request, adminToken, cancellationToken);
    }

    public async Task<List<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetUsersAsync(adminToken, cancellationToken);
    }

    public async Task<UserResponse?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetUserByIdAsync(userId, adminToken, cancellationToken);
    }

    public async Task EnableUserAsync(string userId, bool enabled, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.EnableUserAsync(userId, enabled, adminToken, cancellationToken);
    }
}

