using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AZM.Abyan.Identity.Infrastructure.Middleware;

public class LicenseValidationMiddleware(
    RequestDelegate next,
    IMemoryCache cache,
    ILogger<LicenseValidationMiddleware> logger)
{
    private static readonly string[] _skipped =
        ["/api/license/activate", "/api/license/validate", "/health"];

    public async Task InvokeAsync(HttpContext ctx, IdentityDbContext db)
    {
        var path = ctx.Request.Path.Value ?? "";

        if (_skipped.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        { await next(ctx); return; }

        var licenseIdClaim = ctx.User.FindFirst("license_id")?.Value;
        if (string.IsNullOrEmpty(licenseIdClaim))
        { await next(ctx); return; }

        if (!Guid.TryParse(licenseIdClaim, out var licenseId))
        { await Forbid(ctx, "Invalid license_id claim."); return; }

        // Collect runtime context
        var currentDomain = ctx.Request.Host.Host;  // e.g. "myapp.example.com"
        var currentIp = ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        var cacheKey = $"lic_{licenseId}_{currentDomain}_{currentIp}";

        if (!cache.TryGetValue(cacheKey, out bool isValid))
        {
            var license = await db.Licenses
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == licenseId, ctx.RequestAborted);

            if (!license.IsActive)
            {
                logger.LogWarning("Inactive or revoked license {Id} blocked", licenseId);
                await Forbid(ctx, "License is not active or has been revoked."); return;
            }

            isValid = license is not null
                   && license.IsActive
                   && license.ExpiryDate > DateTime.UtcNow
                   && license.IsDomainAllowed(currentDomain)
                   && license.IsIpAllowed(currentIp);

            if (!isValid && license is not null)
            {
                logger.LogWarning(
                    "License {Id} rejected — Active:{A} Expired:{E} Domain:{D} IP:{I}",
                    licenseId,
                    license.IsActive,
                    license.ExpiryDate <= DateTime.UtcNow,
                    !license.IsDomainAllowed(currentDomain),
                    !license.IsIpAllowed(currentIp));
            }

            cache.Set(cacheKey, isValid, TimeSpan.FromMinutes(5));
        }

        if (!isValid) { await Forbid(ctx, "License is invalid, expired, or does not match this domain/IP."); return; }

        await next(ctx);
    }

    private static Task Forbid(HttpContext ctx, string msg)
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        ctx.Response.ContentType = "text/plain";
        return ctx.Response.WriteAsync(msg);
    }
}
