using System.Security.Claims;
using AZM.Abyan.Identity.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Retrieves the current user's unique identifier (Guid).
    /// The method first checks the HTTP request headers for "X-User-Id".
    /// If a valid Guid is found in the header, it is returned.
    /// Otherwise, it falls back to reading the user identifier from claims
    /// (ClaimTypes.NameIdentifier or "sub").
    /// Returns null if no valid Guid is found.
    /// </summary>

    public Guid? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var headerUserId = httpContext?.Request?.Headers["X-User-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerUserId) && Guid.TryParse(headerUserId, out Guid headerGuid))
        {
            return headerGuid;
        }

        var userIdString = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? httpContext?.User?.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdString, out Guid userId))
        {
            return userId;
        }

        return null;
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
    }

    public string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}