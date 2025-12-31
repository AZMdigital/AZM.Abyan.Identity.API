using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LocalPolicy.DTOs;
using LocalPolicy.Models;
using Microsoft.Extensions.Options;

namespace LocalPolicy.Services;

public class KeycloakAuthService : IKeycloakAuthService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfiguration _config;

    public KeycloakAuthService(HttpClient httpClient, IOptions<KeycloakConfiguration> config)
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

        // Add client secret if configured
        if (!string.IsNullOrEmpty(_config.ClientSecret))
        {
            requestBody.Add(new KeyValuePair<string, string>("client_secret", _config.ClientSecret));
        }

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

