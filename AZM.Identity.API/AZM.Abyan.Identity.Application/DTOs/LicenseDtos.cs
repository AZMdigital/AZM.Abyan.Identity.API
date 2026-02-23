using System;

namespace AZM.Abyan.Identity.Application.DTOs;

public class LicenseFileDto
{
    public string LicenseId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Package { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string Signature { get; set; } = string.Empty;
}

public class ActivateLicenseRequest
{
    public string LicenseFile { get; set; } = string.Empty;
}

public record ActivateLicenseResponse(string Token, Guid LicenseId, DateTime ExpiresAt);

public record ValidateLicenseResponse(bool IsValid, string? Reason = null);
