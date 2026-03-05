using System;

namespace AZM.Abyan.Identity.Application.DTOs;

public class LicenseFileDto
{   
    public string LicenseId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public List<string> ClientNames { get; set; } = new();      
    public string Package { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string Signature { get; set; } = string.Empty;
}

public class ActivateLicenseRequest
{
    public string LicenseFile { get; set; } = string.Empty;
}

public record ActivateLicenseResponse(
    string AccessToken,
    string RefreshToken,
    Guid LicenseId,
    DateTime ExpiresAt);

public class RefreshAccessTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public record ValidateLicenseResponse(bool IsValid, string? Reason = null);
