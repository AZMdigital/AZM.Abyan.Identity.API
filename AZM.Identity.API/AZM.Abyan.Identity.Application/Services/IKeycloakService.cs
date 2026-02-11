using System.Text.Json;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;
using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;

namespace AZM.Abyan.Identity.Application.Services;

public interface IKeycloakService
{
    // Auth
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    // Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
     Task LogoutUserAsync(string userId, CancellationToken cancellationToken = default);


    // Users
    Task<string> CreateUserAsync(CreateUserRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(string userId, UpdateUserRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetUsersAsync(string realm, string adminToken, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task EnableUserAsync(string userId, bool enabled, string adminToken, CancellationToken cancellationToken = default);
    Task ResetUserPasswordAsync(string userId, ResetPasswordRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task SendResetPasswordEmailAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task SendVerifyEmailAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByUsernameAsync(string username, string adminToken, CancellationToken cancellationToken = default);
    Task<List<RealmRoleResponse>> GetUserRealmRolesAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task<List<ClientRoleResponse>> GetUserClientRolesAsync(string userId, string clientId, string adminToken, CancellationToken cancellationToken = default);

    // Roles
    Task<List<ClientRoleResponse>> GetClientRolesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default);
    Task AssignClientRoleToUserAsync(string realm, string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default);
    Task RemoveClientRoleFromUserAsync(string realm, string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default);
    Task CreateClientRoleAsync(string realm, string clientId, CreateClientRoleRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task UpdateClientRoleAsync(string realm, string clientId, string roleName, UpdateClientRoleRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task DeleteClientRoleAsync(string realm, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default);

    // Clients
    Task<List<ClientResponse>> GetClientsAsync(string realm, string adminToken, CancellationToken cancellationToken = default);
    Task<ClientResponse?> GetClientByIdAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default);
    Task<Guid> CreateClientAsync(string realm, CreateClientRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task UpdateClientAsync(string realm, string clientId, UpdateClientRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task DeleteClientAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default);

    // Groups
    Task<List<GroupResponse>> GetGroupsAsync(string adminToken, CancellationToken cancellationToken = default);
    Task<GroupResponse?> GetGroupByIdAsync(string groupId, string adminToken, CancellationToken cancellationToken = default);
    Task CreateGroupAsync(CreateGroupRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task UpdateGroupAsync(string groupId, UpdateGroupRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task DeleteGroupAsync(string groupId, string adminToken, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetGroupMembersAsync(string groupId, string adminToken, CancellationToken cancellationToken = default);
    Task AddUserToGroupAsync(string userId, string groupId, string adminToken, CancellationToken cancellationToken = default);
    Task RemoveUserFromGroupAsync(string userId, string groupId, string adminToken, CancellationToken cancellationToken = default);

    // Realms (Multi-Tenancy)
    Task<List<RealmResponse>> GetAllRealmsAsync(string adminToken, CancellationToken cancellationToken = default);
    Task<RealmResponse?> GetRealmByNameAsync(string realmName, string adminToken, CancellationToken cancellationToken = default);
    Task CreateRealmAsync(CreateRealmRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task UpdateRealmAsync(string realmName, UpdateRealmRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task UpdateRealmPasswordPolicyAsync(string realmName, UpdateRealmPasswordPolicyRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task DeleteRealmAsync(string realmName, string adminToken, CancellationToken cancellationToken = default);

    // Realm Roles
    Task<List<RealmRoleResponse>> GetRealmRolesAsync(string realm, string adminToken, CancellationToken cancellationToken = default);
    Task CreateRealmRoleAsync(CreateRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task UpdateRealmRoleAsync(string realm, string roleName, UpdateRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task DeleteRealmRoleAsync(string realm, string roleName, string adminToken, CancellationToken cancellationToken = default);
    Task AssignRealmRoleToUserAsync(AssignRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task RemoveRealmRoleFromUserAsync(AssignRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task<UserRoleMappingsResponse?> GetUserRoleMappingsAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string[]>> GetClientRoleAttributesAsync(string clientId, string roleName, string adminToken, CancellationToken cancellationToken);
    
    // Protocol Mappers
    Task<ProtocolMapperResponse> CreateProtocolMapperAsync(string realm,string clientId, string clientScopeName, CreateProtocolMapperRequest request, string adminToken, CancellationToken cancellationToken = default);
    Task DisableProtocolMapperAsync(string realm, string clientScopeId, string mapperId, string adminToken, CancellationToken cancellationToken = default);
    
    // Admin token helper
    Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default);
}

