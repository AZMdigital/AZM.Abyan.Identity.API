using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace AZM.Abyan.Identity.Infrastructure.Security.Authorization
{
public sealed class KeycloakUmaAuthorizationService(
    IHttpClientFactory httpClientFactory,
    ITenantProvider tenantProvider,
    IOptions<KeycloakOptions> options,
    IMemoryCache cache,
    IdentityDbContext dbContext,
    IUserService userService)
    : IUmaAuthorizationService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ITenantProvider _tenantProvider = tenantProvider;
        private readonly KeycloakOptions _options = options.Value;
        private readonly IMemoryCache _cache = cache;
        private readonly IdentityDbContext _dbContext = dbContext;
        private readonly IUserService _userService = userService;

        public async Task<bool> IsAuthorizedAsync(
            HttpContext context,
            string accessToken,
            CancellationToken cancellationToken)
        {
            //var tenant = _tenantProvider.GetTenant(context.User);

            var controllerName = context.Request.RouteValues["controller"]?.ToString();
            var actionName = context.Request.RouteValues["action"]?.ToString();
            var method = context.Request.Method;
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.User.FindFirst("sub")?.Value;

            var username = context.User.FindFirst("preferred_username")?.Value
                               ?? context.User.FindFirst(ClaimTypes.Name)?.Value
                               ?? context.User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return false;

            var cacheKey = $"{username}-permissions";
            UserInfoResponse? userInfo = null;
            if (!_cache.TryGetValue(cacheKey, out userInfo))
            {
                userInfo = await _userService.GetCurrentUserInfoAsync(userId, accessToken, cancellationToken);
                if (userInfo != null)
                {
                    _cache.Set(cacheKey, userInfo, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20),
                        Size = 1
                    });
                }
            }
            if (userInfo == null)
                return false;

            // Check if any permission matches the controller and action
            var hasPermission = userInfo.Permissions.Any(p =>
                (p.Resources.Any(r => r.Contains(controllerName!, StringComparison.OrdinalIgnoreCase)) ||
                 p.Name.Contains(controllerName!, StringComparison.OrdinalIgnoreCase)) &&
                (p.Scopes.Any(s => s.Equals(actionName, StringComparison.OrdinalIgnoreCase) || s.Equals(method, StringComparison.OrdinalIgnoreCase))
                 || p.Name.Contains(actionName ?? method, StringComparison.OrdinalIgnoreCase))
            );
            return hasPermission;
        }
        // CallKeycloakAsync is no longer needed
    }

}
