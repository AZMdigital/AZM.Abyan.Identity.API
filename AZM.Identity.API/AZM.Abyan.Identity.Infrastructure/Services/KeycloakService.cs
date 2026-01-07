using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.Extensions.Options;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class KeycloakService(HttpClient httpClient, IOptions<KeycloakConfiguration> config) : IKeycloakService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly KeycloakConfiguration _config = config.Value;

    public async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/realms/{_config.Realm}/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _config.ClientId },
                { "username", _config.AdminUsername },
                { "password", _config.AdminPassword },
                {"client_secret",_config.ClientSecret }
            })
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get admin token");
    }

    #region Authorization Services (UMA)

    public async Task<ResourceDto?> GetResourceAsync(string clientId, string resourceName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/authz/resource-server/resource?name={resourceName}&exact=true";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var resources = await response.Content.ReadFromJsonAsync<List<ResourceDto>>(cancellationToken: cancellationToken);
        return resources?.FirstOrDefault();
    }

    #endregion

    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var tokenEndpoint = $"/realms/{_config.RealmLocal}/protocol/openid-connect/token";

        var requestBody = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", _config.ClientIdLocal),
            new("username", username),
            new("password", password),
            new("client_secret",_config.ClientSecret)
        };

        if (!string.IsNullOrEmpty(_config.ClientSecret))
        {
            requestBody.Add(new KeyValuePair<string, string>("client_secret", _config.ClientSecret));
        }

        var content = new FormUrlEncodedContent(requestBody);
        var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Keycloak authentication failed ({(int)response.StatusCode} {response.StatusCode}): {errorContent}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        return new LoginResponse
        {
            AccessToken = tokenResponse?.AccessToken ?? string.Empty,
            RefreshToken = tokenResponse?.RefreshToken ?? string.Empty,
            ExpiresIn = tokenResponse?.ExpiresIn ?? 0,
            TokenType = tokenResponse?.TokenType ?? "Bearer"
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEndpoint = $"/realms/{_config.Realm}/protocol/openid-connect/token";

        var requestBody = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("client_id", _config.ClientId),
            new("refresh_token", refreshToken)
        };

        var content = new FormUrlEncodedContent(requestBody);
        var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        return new LoginResponse
        {
            AccessToken = tokenResponse?.AccessToken ?? string.Empty,
            RefreshToken = tokenResponse?.RefreshToken ?? string.Empty,
            ExpiresIn = tokenResponse?.ExpiresIn ?? 0,
            TokenType = tokenResponse?.TokenType ?? "Bearer"
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var logoutEndpoint = $"/realms/{_config.Realm}/protocol/openid-connect/logout";

        var requestBody = new List<KeyValuePair<string, string>>
        {
            new("client_id", _config.ClientId),
            new("refresh_token", refreshToken)
        };

        var content = new FormUrlEncodedContent(requestBody);
        await _httpClient.PostAsync(logoutEndpoint, content, cancellationToken);
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users";

        var userPayload = new
        {
            username = request.Username,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = request.Enabled,
            emailVerified = request.EmailVerified,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = request.Password,
                    temporary = false
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(userPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var location = response.Headers.Location?.ToString();
            if (!string.IsNullOrEmpty(location))
            {
                var userId = location.Split('/').Last();
                return userId;
            }
        }

        response.EnsureSuccessStatusCode();
        return string.Empty;
    }

    public async Task<List<UserResponse>> GetUsersAsync(string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>(cancellationToken: cancellationToken);
        return users ?? new List<UserResponse>();
    }

    public async Task<UserResponse?> GetUserByIdAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: cancellationToken);
    }

    public async Task EnableUserAsync(string userId, bool enabled, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}";

        var user = await GetUserByIdAsync(userId, adminToken, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException($"User with id {userId} not found");

        var updatePayload = new
        {
            enabled
        };

        var json = System.Text.Json.JsonSerializer.Serialize(updatePayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ClientRoleResponse>> GetClientRolesAsync(string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.RealmLocal}/clients/{clientId}/roles";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<ClientRoleResponse>>(cancellationToken: cancellationToken);
        return roles ?? [];
    }

    public async Task AssignClientRoleToUserAsync(string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var roleEndpoint = $"/admin/realms/{_config.RealmLocal}/clients/{clientId}/roles/{roleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<ClientRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleName} not found in client {clientId}");

        var assignEndpoint = $"/admin/realms/{_config.RealmLocal}/users/{userId}/role-mappings/clients/{clientId}";

        var rolesPayload = new[]
        {
            new
            {
                id = role.Id,
                name = role.Name,
                description = role.Description,
                composite = role.Composite,
                clientRole = role.ClientRole,
                containerId = role.ContainerId
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(rolesPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var assignRequest = new HttpRequestMessage(HttpMethod.Post, assignEndpoint)
        {
            Content = content
        };
        assignRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(assignRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveClientRoleFromUserAsync(string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var roleEndpoint = $"/admin/realms/{_config.RealmLocal}/clients/{clientId}/roles/{roleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<ClientRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleName} not found in client {clientId}");

        var removeEndpoint = $"/admin/realms/{_config.RealmLocal}/users/{userId}/role-mappings/clients/{clientId}";

        var rolesPayload = new[]
        {
            new
            {
                id = role.Id,
                name = role.Name,
                description = role.Description,
                composite = role.Composite,
                clientRole = role.ClientRole,
                containerId = role.ContainerId
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(rolesPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Delete, removeEndpoint)
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateClientRoleAsync(string clientId, CreateClientRoleRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/roles";

        var role = new
        {
            name = request.Name,
            description = request.Description
        };

        var json = System.Text.Json.JsonSerializer.Serialize(role);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteClientRoleAsync(string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/roles/{roleName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ClientResponse>> GetClientsAsync(string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var clients = await response.Content.ReadFromJsonAsync<List<ClientResponse>>(cancellationToken: cancellationToken);
        return clients ?? new List<ClientResponse>();
    }

    public async Task<JsonElement?> GetClientByIdAsync(string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.RealmLocal}/clients/?clientId={clientId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
    }

    public async Task CreateClientAsync(CreateClientRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients";

        var client = new
        {
            clientId = request.ClientId,
            name = request.Name,
            description = request.Description,
            enabled = request.Enabled,
            protocol = request.Protocol,
            publicClient = request.PublicClient,
            bearerOnly = request.BearerOnly,
            serviceAccountsEnabled = request.ServiceAccountsEnabled,
            authorizationServicesEnabled = request.AuthorizationServicesEnabled,
            redirectUris = request.RedirectUris,
            webOrigins = request.WebOrigins
        };

        var json = System.Text.Json.JsonSerializer.Serialize(client);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateClientAsync(string clientId, UpdateClientRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}";

        var client = new
        {
            name = request.Name,
            description = request.Description,
            enabled = request.Enabled,
            serviceAccountsEnabled = request.ServiceAccountsEnabled,
            authorizationServicesEnabled = request.AuthorizationServicesEnabled,
            redirectUris = request.RedirectUris,
            webOrigins = request.WebOrigins
        };

        var json = System.Text.Json.JsonSerializer.Serialize(client);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteClientAsync(string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<GroupResponse>> GetGroupsAsync(string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/groups";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var groups = await response.Content.ReadFromJsonAsync<List<GroupResponse>>(cancellationToken: cancellationToken);
        return groups ?? new List<GroupResponse>();
    }

    public async Task<GroupResponse?> GetGroupByIdAsync(string groupId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/groups/{groupId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GroupResponse>(cancellationToken: cancellationToken);
    }

    public async Task CreateGroupAsync(CreateGroupRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        // If ParentGroupId is present, we might want to create it as a child. 
        // Keycloak API for child group: POST /admin/realms/{realm}/groups/{id}/children
        // Root group: POST /admin/realms/{realm}/groups

        string endpoint;
        if (!string.IsNullOrEmpty(request.ParentGroupId))
        {
            endpoint = $"/admin/realms/{_config.Realm}/groups/{request.ParentGroupId}/children";
        }
        else
        {
            endpoint = $"/admin/realms/{_config.Realm}/groups";
        }

        var groupPayload = new
        {
            name = request.Name
        };

        var json = System.Text.Json.JsonSerializer.Serialize(groupPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateGroupAsync(string groupId, UpdateGroupRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/groups/{groupId}";

        var groupPayload = new
        {
            name = request.Name
        };

        var json = System.Text.Json.JsonSerializer.Serialize(groupPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteGroupAsync(string groupId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/groups/{groupId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<UserResponse>> GetGroupMembersAsync(string groupId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/groups/{groupId}/members";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var members = await response.Content.ReadFromJsonAsync<List<UserResponse>>(cancellationToken: cancellationToken);
        return members ?? new List<UserResponse>();
    }

    public async Task AddUserToGroupAsync(string userId, string groupId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/groups/{groupId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveUserFromGroupAsync(string userId, string groupId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/groups/{groupId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserAsync(string userId, UpdateUserRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}";

        var userPayload = new
        {
            firstName = request.FirstName,
            lastName = request.LastName,
            email = request.Email
        };

        var json = System.Text.Json.JsonSerializer.Serialize(userPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ResetUserPasswordAsync(string userId, ResetPasswordRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/reset-password";

        var payload = new
        {
            type = "password",
            value = request.NewPassword,
            temporary = request.Temporary
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendVerifyEmailAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/execute-actions-email";

        var actions = new List<string> { "VERIFY_EMAIL" };
        var json = System.Text.Json.JsonSerializer.Serialize(actions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<RealmResponse>> GetAllRealmsAsync(string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = "/admin/realms";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var realms = await response.Content.ReadFromJsonAsync<List<RealmResponse>>(cancellationToken: cancellationToken);
        return realms ?? new List<RealmResponse>();
    }

    public async Task<RealmResponse?> GetRealmByNameAsync(string realmName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realmName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RealmResponse>(cancellationToken: cancellationToken);
    }

    public async Task CreateRealmAsync(CreateRealmRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = "/admin/realms";

        var realmPayload = new
        {
            realm = request.Realm,
            displayName = request.DisplayName,
            enabled = request.Enabled,
            sslRequired = request.SslRequired,
            registrationAllowed = request.RegistrationAllowed,
            loginWithEmailAllowed = request.LoginWithEmailAllowed,
            duplicateEmailsAllowed = request.DuplicateEmailsAllowed
        };

        var json = System.Text.Json.JsonSerializer.Serialize(realmPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRealmAsync(string realmName, UpdateRealmRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realmName}";

        var realmPayload = new
        {
            realm = realmName,
            displayName = request.DisplayName,
            enabled = request.Enabled,
            sslRequired = request.SslRequired,
            registrationAllowed = request.RegistrationAllowed,
            loginWithEmailAllowed = request.LoginWithEmailAllowed,
            duplicateEmailsAllowed = request.DuplicateEmailsAllowed
        };

        var json = System.Text.Json.JsonSerializer.Serialize(realmPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRealmPasswordPolicyAsync(string realmName, UpdateRealmPasswordPolicyRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realmName}";

        var payload = new
        {
            passwordPolicy = request.PasswordPolicy
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRealmAsync(string realmName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realmName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<RealmRoleResponse>> GetRealmRolesAsync(string realm, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/roles";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<RealmRoleResponse>>(cancellationToken: cancellationToken);
        return roles ?? new List<RealmRoleResponse>();
    }

    public async Task CreateRealmRoleAsync(CreateRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{request.Realm}/roles";

        var rolePayload = new
        {
            name = request.Name,
            description = request.Description
        };

        var json = System.Text.Json.JsonSerializer.Serialize(rolePayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Guid> CreateResourceAsync(string clientId, ResourceDto resource, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/authz/resource-server/resource";

        var json = System.Text.Json.JsonSerializer.Serialize(resource);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ResourceDto>(cancellationToken: cancellationToken);
        return created?.Id ?? Guid.Empty;
    }

    public async Task UpdateResourceAsync(string clientId, ResourceDto resource, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/authz/resource-server/resource/{resource.Id}";

        var json = System.Text.Json.JsonSerializer.Serialize(resource);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PolicyDto?> GetPolicyAsync(string clientId, string policyName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/authz/resource-server/policy?name={policyName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var policies = await response.Content.ReadFromJsonAsync<List<PolicyDto>>(cancellationToken: cancellationToken);
        return policies?.FirstOrDefault(p => p.Name == policyName);
    }

    public async Task<string> CreateRolePolicyAsync(string clientId, string policyName, IEnumerable<string> roleNames, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/authz/resource-server/policy/role";

        var rolesConfig = new List<object>();
        // Optimisation: assume roles are fetched outside or simple implementation for now:
        // Getting role ID:
        var roles = await GetClientRolesAsync(clientId, adminToken, cancellationToken);
        foreach(var r in roleNames)
        {
             var roleObj = roles.FirstOrDefault(x => x.Name == r);
             if(roleObj != null)
             {
                 rolesConfig.Add(new { id = roleObj.Id, required = true });
             }
        }

        var payload = new
        {
            name = policyName,
            type = "role",
            logic = "POSITIVE",
            decisionStrategy = "UNANIMOUS",
            config = new
            {
                 roles = System.Text.Json.JsonSerializer.Serialize(rolesConfig)
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<PolicyDto>(cancellationToken: cancellationToken);
        return created?.Id ?? string.Empty;
    }

    public async Task<PermissionDto?> GetPermissionAsync(string clientId, string permissionName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/authz/resource-server/permission?name={permissionName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var permissions = await response.Content.ReadFromJsonAsync<List<PermissionDto>>(cancellationToken: cancellationToken);
        return permissions?.FirstOrDefault(p => p.Name == permissionName);
    }

    public async Task<string> CreateScopePermissionAsync(string clientId, string permissionName, IEnumerable<string> resources, IEnumerable<string> scopes, IEnumerable<string> policies, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/authz/resource-server/permission/scope";

        var payload = new
        {
            name = permissionName,
            type = "scope",
            logic = "POSITIVE",
            decisionStrategy = "UNANIMOUS",
            resources = resources,
            scopes = scopes,
            policies = policies
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<PermissionDto>(cancellationToken: cancellationToken);
        return created?.Id ?? string.Empty;
    }


    public async Task UpdateRealmRoleAsync(string realm, string roleName, UpdateRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/roles/{roleName}";

        var rolePayload = new
        {
            name = request.Name,
            description = request.Description
        };

        var json = System.Text.Json.JsonSerializer.Serialize(rolePayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRealmRoleAsync(string realm, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/roles/{roleName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignRealmRoleToUserAsync(AssignRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var roleEndpoint = $"/admin/realms/{request.Realm}/roles/{request.RoleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<RealmRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {request.RoleName} not found in realm {request.Realm}");

        var assignEndpoint = $"/admin/realms/{request.Realm}/users/{request.UserId}/role-mappings/realm";

        var rolesPayload = new[]
        {
            new
            {
                id = role.Id,
                name = role.Name,
                description = role.Description,
                composite = role.Composite,
                containerId = role.ContainerId
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(rolesPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var assignRequest = new HttpRequestMessage(HttpMethod.Post, assignEndpoint)
        {
            Content = content
        };
        assignRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(assignRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveRealmRoleFromUserAsync(AssignRealmRoleRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var roleEndpoint = $"/admin/realms/{request.Realm}/roles/{request.RoleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<RealmRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {request.RoleName} not found in realm {request.Realm}");

        var removeEndpoint = $"/admin/realms/{request.Realm}/users/{request.UserId}/role-mappings/realm";

        var rolesPayload = new[]
        {
            new
            {
                id = role.Id,
                name = role.Name,
                description = role.Description,
                composite = role.Composite,
                containerId = role.ContainerId
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(rolesPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var removeRequest = new HttpRequestMessage(HttpMethod.Delete, removeEndpoint)
        {
            Content = content
        };
        removeRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(removeRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }


    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}

