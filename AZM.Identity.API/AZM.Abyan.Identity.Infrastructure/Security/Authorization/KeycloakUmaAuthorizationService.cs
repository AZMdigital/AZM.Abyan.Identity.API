using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace AZM.Abyan.Identity.Infrastructure.Security.Authorization
{
    public sealed class KeycloakUmaAuthorizationService(
     IHttpClientFactory httpClientFactory,
     ITenantProvider tenantProvider,
     IOptions<KeycloakOptions> options,
     IMemoryCache cache,
     IdentityDbContext dbContext)
     : IUmaAuthorizationService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ITenantProvider _tenantProvider = tenantProvider;
        private readonly KeycloakOptions _options = options.Value;
        private readonly IMemoryCache _cache = cache;
        private readonly IdentityDbContext _dbContext = dbContext;

        public async Task<bool> IsAuthorizedAsync(
            HttpContext context,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var tenant = _tenantProvider.GetTenant(context.User);

            var path = context.Request.Path.Value!;
            var method = context.Request.Method;

            // Normalize path to find the controller name (e.g. /api/Clients/GetClients -> res:clients)
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string? controllerName = null;
            if (segments.Length > 0)
            {
                // Most routes start with /api, so the controller is the second segment
                if (segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) && segments.Length > 1)
                {
                    controllerName = segments[1];
                }
                else
                {
                    controllerName = segments[0];
                }
            }

            var resourceName = $"res:{controllerName?.ToLower()}";

            // Try to find the resource ID from the database using the mapped name
            var resource = await _dbContext.Resources
                .FirstOrDefaultAsync(r => r.Name == resourceName, cancellationToken);
            
            // If not found by name, try fallback or just the path as resource name
            var resourceIdentifier = resource?.Id.ToString() ?? path;

            var permissionString = $"{resourceIdentifier}#{method}";

            var cacheKey = $"{context.User.Identity?.Name}-{permissionString}";

            if (_cache.TryGetValue(cacheKey, out bool cached))
                return cached;

            var result = await CallKeycloakAsync(
                permissionString,
                accessToken,
                cancellationToken);

            _cache.Set(cacheKey, result, TimeSpan.FromSeconds(20));

            return result;
        }

        private async Task<bool> CallKeycloakAsync(
            string permission,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_options.Authority}/protocol/openid-connect/token");

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:uma-ticket",
                ["audience"] = _options.Audience,
                ["permission"] = permission
            };

            if (!string.IsNullOrEmpty(_options.ClientId))
                parameters["client_id"] = _options.ClientId;

            if (!string.IsNullOrEmpty(_options.ClientSecret))
                parameters["client_secret"] = _options.ClientSecret;

            request.Content = new FormUrlEncodedContent(parameters);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            }

            return response.IsSuccessStatusCode;
        }
    }

}
