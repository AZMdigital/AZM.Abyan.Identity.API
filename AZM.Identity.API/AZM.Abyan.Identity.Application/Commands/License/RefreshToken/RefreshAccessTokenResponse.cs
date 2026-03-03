namespace AZM.Abyan.Identity.Application.Commands.License.RefreshToken;

public record RefreshAccessTokenResponse(
    string AccessToken,
    string RefreshToken,
    Guid LicenseId,
    DateTime ExpiresAt,
    bool Success = true,
    string? Message = null);