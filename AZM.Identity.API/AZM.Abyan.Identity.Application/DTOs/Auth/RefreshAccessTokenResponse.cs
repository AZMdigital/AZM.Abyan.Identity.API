namespace AZM.Abyan.Identity.Application.DTOs.Auth;

public record RefreshAccessTokenResponse(
    string AccessToken,
    string RefreshToken,
    Guid LicenseId,
    DateTime ExpiresAt,
    bool Success = true,
    string? Message = null);