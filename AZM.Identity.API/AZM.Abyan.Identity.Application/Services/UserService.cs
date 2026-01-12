using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Models;
using Microsoft.Extensions.Options;

namespace AZM.Abyan.Identity.Application.Services;

public class UserService : IUserService
{
    private readonly IKeycloakService _keycloakService;
    private readonly KeycloakConfiguration _keycloakConfig;

    public UserService(IKeycloakService keycloakService, IOptions<KeycloakConfiguration> keycloakConfig)
    {
        _keycloakService = keycloakService;
        _keycloakConfig = keycloakConfig.Value;
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.CreateUserAsync(request, adminToken, cancellationToken);
    }

    public async Task UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.UpdateUserAsync(userId, request, adminToken, cancellationToken);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.DeleteUserAsync(userId, adminToken, cancellationToken);
    }

    public async Task<List<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetUsersAsync(_keycloakConfig.Realm, adminToken, cancellationToken);
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

    public async Task ResetUserPasswordAsync(string userId, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.ResetUserPasswordAsync(userId, request, adminToken, cancellationToken);
    }

    public async Task SendVerifyEmailAsync(string userId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        await _keycloakService.SendVerifyEmailAsync(userId, adminToken, cancellationToken);
    }

    public async Task<UserResponse?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        return await _keycloakService.GetUserByUsernameAsync(username, adminToken, cancellationToken);
    }

    public async Task<UserInfoResponse?> GetCurrentUserInfoAsync(string userId, CancellationToken cancellationToken = default)
    {
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        
        // Get user info
        var user = await _keycloakService.GetUserByIdAsync(userId, adminToken, cancellationToken);
        if (user == null)
            return null;

        // Get realm roles
        var realmRoles = await _keycloakService.GetUserRealmRolesAsync(userId, adminToken, cancellationToken);

        // Get all clients and their roles for the user
        var realm = _keycloakConfig.Realm;
        var clients = await _keycloakService.GetClientsAsync(realm, adminToken, cancellationToken);
        var clientRoles = new Dictionary<string, List<ClientRoleResponse>>();

        foreach (var client in clients)
        {
            var roles = await _keycloakService.GetUserClientRolesAsync(userId, client.Id.ToString(), adminToken, cancellationToken);
            if (roles.Any())
            {
                clientRoles[client.ClientId] = roles;
            }
        }

        return new UserInfoResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Enabled = user.Enabled,
            EmailVerified = user.EmailVerified,
            CreatedTimestamp = user.CreatedTimestamp,
            RealmRoles = realmRoles,
            ClientRoles = clientRoles
        };
    }
}

