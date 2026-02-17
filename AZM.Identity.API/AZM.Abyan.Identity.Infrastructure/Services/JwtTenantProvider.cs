using AZM.Abyan.Identity.Application.Services;
using System.Security.Claims;
using System.Text.Json;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public sealed class JwtTenantProvider : ITenantProvider
{
    public string GetTenant(ClaimsPrincipal user)
    {
        var organizationClaim = user.FindFirst("organization")
            ?? throw new UnauthorizedAccessException("Missing organization claim.");

        using var document = JsonDocument.Parse(organizationClaim.Value);

        var root = document.RootElement;

        // organization contains dynamic tenant key
        var tenantObject = root.EnumerateObject().FirstOrDefault().Value;

        if (!tenantObject.TryGetProperty("id", out var idElement))
            throw new UnauthorizedAccessException("Missing tenant id.");

        return idElement.GetString()
            ?? throw new UnauthorizedAccessException("Invalid tenant id.");
    }
}
