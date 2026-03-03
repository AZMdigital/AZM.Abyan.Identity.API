using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Models;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AZM.Abyan.Identity.Application.Services;

public class UserService : IUserService
{
    private readonly IKeycloakService _keycloakService;
    private readonly KeycloakConfiguration _keycloakConfig;
    private readonly IUserPermissionQueryService _userPermissionQueryService;

    public UserService(IKeycloakService keycloakService, IOptions<KeycloakConfiguration> keycloakConfig, IUserPermissionQueryService userPermissionQueryService)
    {
        _keycloakService = keycloakService;
        _keycloakConfig = keycloakConfig.Value;
        _userPermissionQueryService = userPermissionQueryService;
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


    public async Task<UserInfoResponse?> GetCurrentUserInfoAsync(string userId, string accessToken, CancellationToken cancellationToken = default)
    {
        // ===== Parse Organizations from JWT =====
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
      
        var orgClaim = jwt.Claims.FirstOrDefault(c => c.Type == "AbyanOrganization" || c.Type=="Organization")?.Value;
        var organizationSummaries = new List<OrganizationSummary>();
        if (!string.IsNullOrEmpty(orgClaim))
        {
            var orgs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, OrganizationInfo>>(orgClaim);
            if (orgs != null)
            {
                organizationSummaries = orgs.Select(o => new OrganizationSummary
                {
                    Id = o.Value.id,
                    Name = o.Key
                }).ToList();
            }
        }
        // ===== Admin token for user info & roles =====
        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

        // Get user info
        var user = await _keycloakService.GetUserByIdAsync(userId, adminToken, cancellationToken);
        if (user == null) return null;

        // Get all role mappings (realm + client)
        var mappings = await _keycloakService.GetUserRoleMappingsAsync(userId, adminToken, cancellationToken);

        // Realm roles
        var realmRoles = mappings.RealmMappings?.Select(r => new RealmRoleResponse
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Composite = r.Composite,
            ContainerId = r.ContainerId
        }).ToList()
                         ?? new List<RealmRoleResponse>();
        // Client roles
        var clientRoles = new Dictionary<string, List<ClientRoleResponse>>();
        if (mappings.ClientMappings != null)
        {
            foreach (var kvp in mappings.ClientMappings)
            {
                clientRoles[kvp.Key] = kvp.Value.Mappings;

                // Optional: get attributes if needed
                foreach (var role in kvp.Value.Mappings)
                {
                    role.Attributes = await _keycloakService.GetClientRoleAttributesAsync(
                        kvp.Value.Id, role.Name, adminToken, cancellationToken);
                }
            }
        }
        // Permissions, Scopes, Resources, Policies: fetch only for the current user from the database via query service
        var permissions = await _userPermissionQueryService.GetUserPermissionsAsync(userId, cancellationToken);
        //var policies = await _userPermissionQueryService.GetUserPoliciesAsync(userId, cancellationToken);
        //var resources = await _userPermissionQueryService.GetUserResourcesAsync(userId, cancellationToken);
        //var scopes = await _userPermissionQueryService.GetUserScopesAsync(userId, cancellationToken);
        // Return full user info
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
            ClientRoles = clientRoles,
            Organizations = organizationSummaries,
            Permissions = permissions
            //,
            //Scopes = scopes,
            //Resources = resources,
            //Policies = policies
        };
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return false;
        }

        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
        var user = await _keycloakService.GetUserByUsernameAsync(request.Username, adminToken, cancellationToken);

        if (user == null)
        {
            return false;
        }

        await _keycloakService.SendResetPasswordEmailAsync(user.Id, adminToken, cancellationToken);
        return true;
    }
}

