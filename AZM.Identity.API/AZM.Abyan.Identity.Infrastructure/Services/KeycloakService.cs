using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfiguration _config;

    public KeycloakService(HttpClient httpClient, IOptions<KeycloakConfiguration> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var tokenEndpoint = $"/realms/{_config.Realm}/protocol/openid-connect/token";

        var requestBody = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", _config.ClientId),
            new("username", username),
            new("password", password)
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
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/roles";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<ClientRoleResponse>>(cancellationToken: cancellationToken);
        return roles ?? new List<ClientRoleResponse>();
    }

    public async Task AssignClientRoleToUserAsync(string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var roleEndpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/roles/{roleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<ClientRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleName} not found in client {clientId}");

        var assignEndpoint = $"/admin/realms/{_config.Realm}/users/{userId}/role-mappings/clients/{clientId}";

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
        var roleEndpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/roles/{roleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<ClientRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleName} not found in client {clientId}");

        var removeEndpoint = $"/admin/realms/{_config.Realm}/users/{userId}/role-mappings/clients/{clientId}";

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

    public async Task<ClientResponse?> GetClientByIdAsync(string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ClientResponse>(cancellationToken: cancellationToken);
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

    public async Task AddUserToGroupAsync(string userId, string groupId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/groups/{groupId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default)
    {
        var loginResponse = await LoginAsync(_config.AdminUsername, _config.AdminPassword, cancellationToken);
        return loginResponse.AccessToken;
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

