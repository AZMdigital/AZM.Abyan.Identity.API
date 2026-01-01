using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;

namespace AZM.Abyan.Identity.Application.Services;

public interface IKeycloakService
{
    // Auth
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    // Users
    Task<string> CreateUserAsync(CreateUserRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetUsersAsync(string adminToken, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task EnableUserAsync(string userId, bool enabled, string adminToken, CancellationToken cancellationToken = default);

    // Roles
    Task<List<ClientRoleResponse>> GetClientRolesAsync(string clientId, string adminToken, CancellationToken cancellationToken = default);
    Task AssignClientRoleToUserAsync(string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default);
    Task RemoveClientRoleFromUserAsync(string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default);

    // Clients
    Task<List<ClientResponse>> GetClientsAsync(string adminToken, CancellationToken cancellationToken = default);
    Task<ClientResponse?> GetClientByIdAsync(string clientId, string adminToken, CancellationToken cancellationToken = default);

    // Groups
    Task<List<GroupResponse>> GetGroupsAsync(string adminToken, CancellationToken cancellationToken = default);
    Task AddUserToGroupAsync(string userId, string groupId, string adminToken, CancellationToken cancellationToken = default);

    // Admin token helper
    Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default);
}

