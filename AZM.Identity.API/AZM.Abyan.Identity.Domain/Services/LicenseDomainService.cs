using AZM.Abyan.Identity.Domain.Entities;

namespace AZM.Abyan.Identity.Domain.Services;

/// <summary>
/// Pure domain-level license guard logic — no infrastructure dependencies.
/// Call these *after* RSA signature has already been verified.
/// </summary>
public static class LicenseDomainService
{
    public static void EnsureIsActive(License license)
    {
        if (!license.IsActive)
            throw new InvalidOperationException("License is not active or has been revoked.");
    }

    public static void EnsureNotExpired(License license)
    {
        if (license.ExpiryDate <= DateTime.UtcNow)
            throw new InvalidOperationException("License has expired.");
    }
}
