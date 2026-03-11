using AZM.Abyan.Identity.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class KeycloakVerifier(HttpClient http, IConfiguration config) : IKeycloakVerifier
{
    private readonly SemaphoreSlim _sem = new(1, 1);
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task<bool> TenantExistsAsync(string realmName, CancellationToken ct = default)
    {
        var token = await GetAdminTokenAsync(ct);
        var baseUrl = config["Keycloak:BaseUrl"]!.TrimEnd('/');
        var url = $"{baseUrl}/admin/realms/{realmName}";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await http.SendAsync(req, ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> ClientExistsAsync(string realmName, string clientName, CancellationToken ct = default)
    {
        var token = await GetAdminTokenAsync(ct);
        var baseUrl = config["Keycloak:BaseUrl"]!.TrimEnd('/');
        var url = $"{baseUrl}/admin/realms/{realmName}/clients?clientId={Uri.EscapeDataString(clientName)}";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return false;

        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetArrayLength() > 0;
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _sem.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            var baseUrl = config["Keycloak:BaseUrl"]!.TrimEnd('/');
            var realm = config["Keycloak:AdminRealm"]!;
            var form = new FormUrlEncodedContent(new Dictionary<string, string> // Explicit fully qualified generic type because it might clash
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = config["Keycloak:AdminClientId"]!,
                ["client_secret"] = config["Keycloak:AdminClientSecret"]!,
            });

            var resp = await http.PostAsync($"{baseUrl}/realms/{realm}/protocol/openid-connect/token", form, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            _cachedToken = root.GetProperty("access_token").GetString()!;
            var expiresIn = root.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 30); // 30s buffer

            return _cachedToken;
        }
        finally { _sem.Release(); }
    }
}
