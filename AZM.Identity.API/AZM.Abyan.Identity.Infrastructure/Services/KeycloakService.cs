using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;
using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using static System.Net.WebRequestMethods;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfiguration _config;
    private readonly IConfiguration _configuration;

    public KeycloakService(HttpClient httpClient, IOptions<KeycloakConfiguration> config, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _configuration = configuration;
    }

    public async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default)
    {
        // Use KeycloakAdmin configuration for admin token from new structure
        var adminConfig = _configuration.GetSection("KeycloakConfigurations:Realms:master:KeycloakAdmin").Get<KeycloakConfiguration>();
        if (adminConfig == null)
        {
            throw new InvalidOperationException("KeycloakAdmin configuration not found at KeycloakConfigurations:Realms:master:KeycloakAdmin");
        }

        // Set Realm to "master" if not specified in config
        if (string.IsNullOrEmpty(adminConfig.Realm))
        {
            adminConfig.Realm = "master";
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/realms/{adminConfig.Realm}/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", adminConfig.ClientId },
                { "username", adminConfig.AdminUsername },
                { "password", adminConfig.AdminPassword }
            })
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get admin token");
    }

    #region Authorization Services (UMA)

    public async Task<ResourceDto?> GetResourceAsync(string realm, string clientId, string resourceName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/resource?name={resourceName}&exact=true";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var resources = await response.Content.ReadFromJsonAsync<List<ResourceDto>>(cancellationToken: cancellationToken);
        return resources?.FirstOrDefault();
    }

    public async Task<List<ResourceDto>> GetAllResourcesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/resource";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var resources = await response.Content.ReadFromJsonAsync<List<ResourceDto>>(cancellationToken: cancellationToken);
        return resources ?? new List<ResourceDto>();
    }

    public async Task<List<ScopeDto>> GetAllScopesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/scope";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var scopes = await response.Content.ReadFromJsonAsync<List<ScopeDto>>(cancellationToken: cancellationToken);
        return scopes ?? new List<ScopeDto>();
    }

    public async Task<List<PolicyDto>> GetAllPoliciesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/policy";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var policies = await response.Content.ReadFromJsonAsync<List<PolicyDto>>(cancellationToken: cancellationToken);
        return policies ?? new List<PolicyDto>();
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/permission";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var permissions = await response.Content.ReadFromJsonAsync<List<PermissionDto>>(cancellationToken: cancellationToken);
        return permissions ?? new List<PermissionDto>();
    }

    #endregion

    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var userId = await GetUserIdAsync(username, adminToken, cancellationToken);
        var clientWithRoles = await GetClientWithRolesAsync(userId, adminToken, cancellationToken);

        if (clientWithRoles == null)
            throw new Exception("User has no client roles");

        var clientSecret = await GetClientSecretAsync(
            clientWithRoles.ClientInternalId,
            adminToken,
            cancellationToken);

        var token = await GetTokenAsync(
            username,
            password,
            clientWithRoles.ClientId,
            clientSecret,
            cancellationToken);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresIn = token.ExpiresIn,
            TokenType = token.TokenType
        };
    }
    private async Task<string> GetUserIdAsync(
      string username,
      string adminToken,
      CancellationToken ct)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/{_config.Realm}/users?username={username}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(ct);
        return users?.FirstOrDefault()?.Id
               ?? throw new Exception("User not found");
    }

    private async Task<ClientWithRoles?> GetClientWithRolesAsync(
     string userId,
     string adminToken,
     CancellationToken ct)
    {
        var clientsRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/{_config.Realm}/clients");

        clientsRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var clientsResponse = await _httpClient.SendAsync(clientsRequest, ct);
        clientsResponse.EnsureSuccessStatusCode();

        var clients = await clientsResponse.Content.ReadFromJsonAsync<List<ClientDto>>(ct);

        foreach (var client in clients!)
        {
            var rolesRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"/admin/realms/{_config.Realm}/users/{userId}/role-mappings/clients/{client.Id}");

            rolesRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);

            var rolesResponse = await _httpClient.SendAsync(rolesRequest, ct);
            if (!rolesResponse.IsSuccessStatusCode)
                continue;

            var roles = await rolesResponse.Content.ReadFromJsonAsync<List<RoleDto>>(ct);

            if (roles != null && roles.Any())
            {
                return new ClientWithRoles
                {
                    ClientId = client.ClientId,
                    ClientInternalId = client.Id,
                    Roles = roles.Select(r => r.Name).ToList()
                };
            }
        }

        return null;
    }

    private async Task<string> GetClientSecretAsync(
      string clientInternalId,
      string adminToken,
      CancellationToken ct)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/{_config.Realm}/clients/{clientInternalId}/client-secret");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var secret = await response.Content.ReadFromJsonAsync<ClientSecretDto>(ct);
        return secret?.Value ?? throw new Exception("Client secret not found");
    }

    private async Task<TokenResponse> GetTokenAsync(
    string username,
    string password,
    string clientId,
    string clientSecret,
    CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"/realms/{_config.Realm}/protocol/openid-connect/token";

        var body = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", clientId),
            new("username", username),
            new("password", password)
        };
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            body.Add(new("client_secret", clientSecret));
        }
        var response = await _httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(body),
            cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        return result ?? throw new Exception("Token error");
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

    //public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    //{
    //    var logoutEndpoint = $"/realms/{_config.Realm}/protocol/openid-connect/logout";

    //    var requestBody = new List<KeyValuePair<string, string>>
    //    {
    //        new("client_id", _config.ClientId),
    //        new("refresh_token", refreshToken)
    //    };

    //    var content = new FormUrlEncodedContent(requestBody);
    //    await _httpClient.PostAsync(logoutEndpoint, content, cancellationToken);
    //}
    public async Task LogoutUserAsync(string userId, CancellationToken cancellationToken = default)
{
    var adminToken = await GetAdminTokenAsync(cancellationToken);

    var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/logout";

    var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
    request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", adminToken);

    var response = await _httpClient.SendAsync(request, cancellationToken);

    response.EnsureSuccessStatusCode();
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
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        if (!string.IsNullOrEmpty(location))
        {
            var userId = location.Split('/').Last();

            // ✅Delete default role
            await RemoveAllDefaultRolesAsync(userId, adminToken, cancellationToken);

            return userId;
        }

        return string.Empty;
    }

    private async Task RemoveAllDefaultRolesAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
    {
        var realm = _config.Realm;

        // 1. Get default roles realm
        var request = new HttpRequestMessage(HttpMethod.Get, $"/admin/realms/{realm}/roles");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var rolesResponse = await _httpClient.SendAsync(request, cancellationToken);
        rolesResponse.EnsureSuccessStatusCode();

        if (!rolesResponse.IsSuccessStatusCode)
        {
            var content = await rolesResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to get roles: {rolesResponse.StatusCode}, {content}");
            return;
        }
        rolesResponse.EnsureSuccessStatusCode();

        var rolesJson = await rolesResponse.Content.ReadAsStringAsync(cancellationToken);
        var roles = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(rolesJson);

        // 2. Get role which start by "default-roles"
        var defaultRoles = roles.EnumerateArray()
            .Where(r => r.GetProperty("name").GetString()?.StartsWith("default-roles") == true)
            .Select(r => new
            {
                id = r.GetProperty("id").GetString(),
                name = r.GetProperty("name").GetString()
            })
            .ToArray();

        if (defaultRoles.Length == 0) return;

        // 3. Delete Them
        var json = System.Text.Json.JsonSerializer.Serialize(defaultRoles);
        var requestnew = new HttpRequestMessage(HttpMethod.Delete, $"/admin/realms/{realm}/users/{userId}/role-mappings/realm")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        requestnew.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var deleteResponse = await _httpClient.SendAsync(requestnew, cancellationToken);
        deleteResponse.EnsureSuccessStatusCode();
    }


    public async Task<List<UserResponse>> GetUsersAsync(string realm, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/users";

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

    public async Task<List<ClientRoleResponse>> GetClientRolesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/roles";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<ClientRoleResponse>>(cancellationToken: cancellationToken);
        return roles ?? [];
    }

    public async Task AssignClientRoleToUserAsync(string realm, string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var roleEndpoint = $"/admin/realms/{realm}/clients/{clientId}/roles/{roleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        //roleResponse.EnsureSuccessStatusCode();
        var body = await roleResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!roleResponse.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Keycloak error {(int)roleResponse.StatusCode}: {body}"
            );
        }
        var role = await roleResponse.Content.ReadFromJsonAsync<ClientRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleName} not found in client {clientId}");

        var assignEndpoint = $"/admin/realms/{realm}/users/{userId}/role-mappings/clients/{clientId}";

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

    public async Task RemoveClientRoleFromUserAsync(string realm, string userId, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var roleEndpoint = $"/admin/realms/{realm}/clients/{clientId}/roles/{roleName}";

        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<ClientRoleResponse>(cancellationToken: cancellationToken);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleName} not found in client {clientId}");

        var removeEndpoint = $"/admin/realms/{realm}/users/{userId}/role-mappings/clients/{clientId}";

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

    public async Task CreateClientRoleAsync(string realm, string clientId, CreateClientRoleRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/roles";

        var role = new
        {
            name = request.Name,
            description = request.Description,
            attributes = request.Attributes ?? new Dictionary<string, string[]>()
        };

        var json = System.Text.Json.JsonSerializer.Serialize(role);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        // DEBUG: Capture response details as JSON string for debugging
        var debugResponseJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
            Content = await response.Content.ReadAsStringAsync(cancellationToken),
            RequestUri = response.RequestMessage?.RequestUri?.ToString(),
            RequestMethod = response.RequestMessage?.Method?.ToString()
        });
        var debugResponseForInspector = debugResponseJson; // Variable for debugger inspection

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateClientRoleAsync(string realm, string clientId, string roleName, UpdateClientRoleRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/roles/{roleName}";

        var role = new
        {
            name = request.Name,
            description = request.Description,
            attributes = request.Attributes ?? new Dictionary<string, string[]>()
        };

        var json = System.Text.Json.JsonSerializer.Serialize(role);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteClientRoleAsync(string realm, string clientId, string roleName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/roles/{roleName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ClientResponse>> GetClientsAsync(string realm, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var clients = await response.Content.ReadFromJsonAsync<List<ClientResponse>>(cancellationToken: cancellationToken);
        return clients ?? new List<ClientResponse>();
    }

    public async Task<ClientResponse?> GetClientByIdAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients?clientId={clientId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var clients = await response.Content
            .ReadFromJsonAsync<List<ClientResponse>>(cancellationToken: cancellationToken);

        return clients?.FirstOrDefault();
    }


    public async Task<Guid> CreateClientAsync(string realm, CreateClientRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients";

        //var client = new
        //{
        //    clientId = request.Name,
        //    name = request.Name,
        //    description = request.Description,
        //    enabled = true,
        //    protocol = "openid-connect",
        //    publicClient = false,
        //    bearerOnly = false,
        //    serviceAccountsEnabled = true,
        //    authorizationServicesEnabled = true,
        //    redirectUris = Array.Empty<string>(),
        //    webOrigins = Array.Empty<string>()
        //};
        var client = new
        {
            clientId = request.Name,
            name = request.Name,
            description = request.Description,
            enabled = true,
            protocol = "openid-connect",
            publicClient = false,
            bearerOnly = false,
            serviceAccountsEnabled = false,
            authorizationServicesEnabled = false,
            redirectUris = request.RedirectUris,
            webOrigins = Array.Empty<string>(),
            standardFlowEnabled = true,
            implicitFlowEnabled = false,
            directAccessGrantsEnabled = true
        };

        var json = System.Text.Json.JsonSerializer.Serialize(client);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Keycloak Error ({(int)response.StatusCode}): {responseBody}");
        }
        response.EnsureSuccessStatusCode();

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/admin/realms/{_config.Realm}/clients?clientId={client.clientId}");

        getRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var getResponse = await _httpClient.SendAsync(getRequest, cancellationToken);
        getResponse.EnsureSuccessStatusCode();

        var clients = await getResponse.Content.ReadFromJsonAsync<List<KeycloakClient>>(cancellationToken: cancellationToken);

        return Guid.Parse(clients!.First().Id);
    }

    public async Task UpdateClientAsync(string realm, string clientId, UpdateClientRequest request, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}";

        var client = new
        {
            clientId = request.Name,
            name = request.Name,
            description = request.Description,
            enabled = true,
            protocol = "openid-connect",
            publicClient = true,
            bearerOnly = false,
            serviceAccountsEnabled = false,
            authorizationServicesEnabled = false,
            redirectUris = request.RedirectUris,
            webOrigins = Array.Empty<string>(),
            standardFlowEnabled = true,
            implicitFlowEnabled = false,
            directAccessGrantsEnabled = false
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

    public async Task DeleteClientAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}";

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
            //  temporary = request.Temporary
            temporary = false
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

    public async Task SendResetPasswordEmailAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/execute-actions-email";

        var actions = new List<string> { "UPDATE_PASSWORD" };
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

    public async Task<UserResponse?> GetUserByUsernameAsync(string username, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users?username={Uri.EscapeDataString(username)}&exact=true";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>(cancellationToken: cancellationToken);
        return users?.FirstOrDefault();
    }

    public async Task<List<RealmRoleResponse>> GetUserRealmRolesAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/role-mappings/realm";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<RealmRoleResponse>>(cancellationToken: cancellationToken);
        return roles ?? new List<RealmRoleResponse>();
    }

    public async Task<List<ClientRoleResponse>> GetUserClientRolesAsync(string userId, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/role-mappings/clients/{clientId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        //var json = await response.Content.ReadAsStringAsync(cancellationToken);
        //var result = JsonDocument.Parse(json).RootElement;
        var roles = await response.Content.ReadFromJsonAsync<List<ClientRoleResponse>>(cancellationToken: cancellationToken);
        return roles ?? new List<ClientRoleResponse>();
    }
    public async Task<UserRoleMappingsResponse?> GetUserRoleMappingsAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/users/{userId}/role-mappings";
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<UserRoleMappingsResponse>(cancellationToken: cancellationToken);
    }
    public async Task<Dictionary<string, string[]>> GetClientRoleAttributesAsync(string clientId, string roleName, string adminToken, CancellationToken cancellationToken)
    {
        var endpoint = $"/admin/realms/{_config.Realm}/clients/{clientId}/roles/{roleName}";
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken));

        return doc.RootElement.TryGetProperty("attributes", out var attrs)
            ? JsonSerializer.Deserialize<Dictionary<string, string[]>>(attrs.GetRawText())!
            : new();
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

    public async Task<Guid> CreateResourceAsync(string realm, string clientId, ResourceDto resource, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/resource";

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

    public async Task UpdateResourceAsync(string realm, string clientId, ResourceDto resource, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/resource/{resource.Id}";

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

    public async Task<ResourceDto?> GetResourceByIdAsync(string realm, string clientId, Guid resourceId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/resource/{resourceId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var resource = await response.Content.ReadFromJsonAsync<ResourceDto>(cancellationToken: cancellationToken);
        return resource;
    }

    public async Task DeleteResourceAsync(string realm, string clientId, Guid resourceId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/resource/{resourceId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ScopeDto?> GetScopeAsync(string realm, string clientId, string scopeName, string adminToken, CancellationToken cancellationToken = default)
    {
        var scopes = await GetAllScopesAsync(realm, clientId, adminToken, cancellationToken);
        return scopes.FirstOrDefault(s => s.Name == scopeName);
    }

    public async Task<ScopeDto?> GetScopeByIdAsync(string realm, string clientId, string scopeId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/scope/{scopeId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var scope = await response.Content.ReadFromJsonAsync<ScopeDto>(cancellationToken: cancellationToken);
        return scope;
    }

    public async Task<string> CreateScopeAsync(string realm, string clientId, ScopeDto scope, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/scope";

        var json = System.Text.Json.JsonSerializer.Serialize(scope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ScopeDto>(cancellationToken: cancellationToken);
        return created?.Name ?? string.Empty;
    }

    public async Task UpdateScopeAsync(string realm, string clientId, string scopeId, ScopeUpdateDto scope, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint =
     $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/scope/{scopeId}";

        var json = JsonSerializer.Serialize(scope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Keycloak error {(int)response.StatusCode}: {body}");
        }
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteScopeAsync(string realm, string clientId, string scopeId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/scope/{scopeId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Keycloak error {(int)response.StatusCode}: {body}"
            );
        }
        response.EnsureSuccessStatusCode();
    }

    public async Task<PolicyDto?> GetPolicyByIdAsync(string realm, string clientId, string policyId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/policy/{policyId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var policy = await response.Content.ReadFromJsonAsync<PolicyDto>(cancellationToken: cancellationToken);
        return policy;
    }

    public async Task UpdatePolicyAsync(string realm, string clientId, string policyId, PolicyDto policy, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/policy/{policyId}";

        var json = System.Text.Json.JsonSerializer.Serialize(policy);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePolicyAsync(string realm, string clientId, string policyId, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/policy/{policyId}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PolicyDto?> GetPolicyAsync(string realm, string clientId, string policyName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/policy?name={policyName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var policies = await response.Content.ReadFromJsonAsync<List<PolicyDto>>(cancellationToken: cancellationToken);
        return policies?.FirstOrDefault(p => p.Name == policyName);
    }

    public async Task<string> CreateRolePolicyAsync(
     string realm,
     string clientId,
     string policyName,
     IEnumerable<string> roleNames,
     string adminToken,
     CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/policy/role";
        var rolesConfig = new List<object>();
        var roles = await GetClientRolesAsync(realm, clientId, adminToken, cancellationToken);
        foreach (var r in roleNames)
        {
            var roleObj = roles.FirstOrDefault(x => x.Name == r);
            if (roleObj != null)
            {
                rolesConfig.Add(new
                {
                    id = roleObj.Id,
                    required = true
                });
            }
        }

        var payload = new
        {
            name = policyName,
            description = $"Role policy for {policyName}",
            type = "role",
            logic = "POSITIVE",
            decisionStrategy = "UNANIMOUS",
            roles = rolesConfig
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Keycloak error {(int)response.StatusCode}: {body}");

        var created = await response.Content.ReadFromJsonAsync<PolicyDto>(cancellationToken: cancellationToken);
        return created?.Id ?? string.Empty;
    }
    public async Task<PermissionDto?> GetPermissionAsync(string realm, string clientId, string permissionName, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/permission?name={permissionName}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var permissions = await response.Content.ReadFromJsonAsync<List<PermissionDto>>(cancellationToken: cancellationToken);
        return permissions?.FirstOrDefault(p => p.Name == permissionName);
    }

    public async Task<string> CreateScopePermissionAsync(string realm, string clientId, string permissionName, IEnumerable<string> resources, IEnumerable<string> scopes, IEnumerable<string> policies, string adminToken, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/admin/realms/{realm}/clients/{clientId}/authz/resource-server/permission/scope";

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
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Keycloak error {(int)response.StatusCode}: {body}");
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

    public async Task<ProtocolMapperResponse> CreateProtocolMapperAsync(
    string realm,
    string clientId,
    string clientScopeName,
    CreateProtocolMapperRequest request,
    string adminToken,
    CancellationToken cancellationToken = default)
    {
        // ===============================
        // 0️⃣ Get or Create Client Scope
        // ===============================
        var getScopeEndpoint =
            $"/admin/realms/{realm}/client-scopes?search={clientScopeName}";

        var getScopeRequest = new HttpRequestMessage(HttpMethod.Get, getScopeEndpoint);
        getScopeRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var getScopeResponse = await _httpClient.SendAsync(getScopeRequest, cancellationToken);
        getScopeResponse.EnsureSuccessStatusCode();

        var scopes = await getScopeResponse.Content
            .ReadFromJsonAsync<List<ClientScopeResponse>>(cancellationToken: cancellationToken);

        var scope = scopes?.FirstOrDefault(s => s.Name == clientScopeName);

        if (scope == null)
        {
            var createScopePayload = new
            {
                name = clientScopeName,
                protocol = "openid-connect"
            };

            var jsonScope = System.Text.Json.JsonSerializer.Serialize(createScopePayload);
            var scopeContent = new StringContent(jsonScope, Encoding.UTF8, "application/json");

            var createScopeRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/admin/realms/{realm}/client-scopes")
            {
                Content = scopeContent
            };
            createScopeRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createScopeResponse =
                await _httpClient.SendAsync(createScopeRequest, cancellationToken);
            createScopeResponse.EnsureSuccessStatusCode();

            var location = createScopeResponse.Headers.Location?.ToString()
                ?? throw new InvalidOperationException("Client scope created but no location returned");

            var scopeId = location.Split('/').Last();

            scope = new ClientScopeResponse
            {
                Id = scopeId,
                Name = clientScopeName
            };
        }

        // ===============================
        // 1️⃣ Create Protocol Mapper
        // ===============================
        var mapperEndpoint =
            $"/admin/realms/{realm}/client-scopes/{scope.Id}/protocol-mappers/models";

        var mapperPayload = new
        {
            name = request.Name,
            protocol = "openid-connect",
            protocolMapper = "oidc-hardcoded-claim-mapper",
            config = new Dictionary<string, string>
            {
                ["claim.value"] = request.ClaimValue,
                ["claim.name"] = request.TokenClaimName,
                ["jsonType.label"] = "String",

                ["id.token.claim"] = request.AddToIdToken.ToString().ToLowerInvariant(),
                ["access.token.claim"] = request.AddToAccessToken.ToString().ToLowerInvariant(),
                ["userinfo.token.claim"] = request.AddToUserInfo.ToString().ToLowerInvariant()
            }
        };

        var jsonMapper = System.Text.Json.JsonSerializer.Serialize(mapperPayload);
        var mapperContent = new StringContent(jsonMapper, Encoding.UTF8, "application/json");

        var createMapperRequest = new HttpRequestMessage(HttpMethod.Post, mapperEndpoint)
        {
            Content = mapperContent
        };
        createMapperRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createMapperResponse =
            await _httpClient.SendAsync(createMapperRequest, cancellationToken);
        createMapperResponse.EnsureSuccessStatusCode();

        var mapperLocation = createMapperResponse.Headers.Location?.ToString()
            ?? throw new InvalidOperationException("Mapper created but no location returned");

        var mapperId = mapperLocation.Split('/').Last();

        // ===============================
        // 2️⃣ Assign Scope to Client (DEFAULT)
        // ===============================
        var assignEndpoint =
            $"/admin/realms/{realm}/clients/{clientId}/default-client-scopes/{scope.Id}";

        var assignRequest = new HttpRequestMessage(HttpMethod.Put, assignEndpoint);
        assignRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var assignResponse =
            await _httpClient.SendAsync(assignRequest, cancellationToken);
        assignResponse.EnsureSuccessStatusCode();

        // ===============================
        // 3️⃣ Return Mapper
        // ===============================
        var getMapperEndpoint =
            $"/admin/realms/{realm}/client-scopes/{scope.Id}/protocol-mappers/models/{mapperId}";

        var getMapperRequest = new HttpRequestMessage(HttpMethod.Get, getMapperEndpoint);
        getMapperRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var getMapperResponse =
            await _httpClient.SendAsync(getMapperRequest, cancellationToken);
        getMapperResponse.EnsureSuccessStatusCode();

        return await getMapperResponse.Content
            .ReadFromJsonAsync<ProtocolMapperResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve mapper");
    }

    public async Task DisableProtocolMapperAsync(string realm, string clientScopeId, string mapperId, string adminToken, CancellationToken cancellationToken = default)
    {
        // First get the current mapper to preserve other settings
        var getEndpoint = $"/admin/realms/{realm}/client-scopes/{clientScopeId}/protocol-mappers/models/{mapperId}";
        var getRequest = new HttpRequestMessage(HttpMethod.Get, getEndpoint);
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var getResponse = await _httpClient.SendAsync(getRequest, cancellationToken);
        getResponse.EnsureSuccessStatusCode();

        var mapper = await getResponse.Content.ReadFromJsonAsync<ProtocolMapperResponse>(cancellationToken: cancellationToken);
        if (mapper == null)
        {
            throw new KeyNotFoundException($"Protocol mapper {mapperId} not found");
        }

        // Update config to disable all token claims
        mapper.Config["id.token.claim"] = "false";
        mapper.Config["access.token.claim"] = "false";
        mapper.Config["userinfo.token.claim"] = "false";

        // Update the mapper
        var updateEndpoint = $"/admin/realms/{realm}/client-scopes/{clientScopeId}/protocol-mappers/models/{mapperId}";
        var updatePayload = new
        {
            id = mapper.Id,
            name = mapper.Name,
            protocol = mapper.Protocol,
            protocolMapper = mapper.ProtocolMapper,
            config = mapper.Config
        };

        var json = System.Text.Json.JsonSerializer.Serialize(updatePayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, updateEndpoint)
        {
            Content = content
        };
        updateRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var updateResponse = await _httpClient.SendAsync(updateRequest, cancellationToken);
        updateResponse.EnsureSuccessStatusCode();
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

