using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace AZM.Abyan.Identity.Application.Services;

public class TokenService(
    IJwtIssuerService jwtIssuer,
    IRefreshTokenRepository refreshTokenRepo,
    ILicenseRepository licenseRepo,
    ILogger<TokenService> logger,
    IStringLocalizer<SharedResource> localizer)
{
    private readonly IJwtIssuerService jwtIssuer = jwtIssuer;
    private readonly IRefreshTokenRepository refreshTokenRepo = refreshTokenRepo;
    private readonly ILicenseRepository licenseRepo = licenseRepo;
    private readonly ILogger<TokenService> _logger = logger;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<ActivateLicenseResponse> ActivateLicenseAsync(License license, List<Client> clients, CancellationToken ct)
    {
        // Generate refresh token
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        var refreshTokenValue = jwtIssuer.GenerateRefreshToken();
        var refreshToken = Domain.Entities.RefreshToken.Create(
            license.Id, refreshTokenValue, refreshTokenExpiry);

        await refreshTokenRepo.AddAsync(refreshToken, ct);

        // Return response with both tokens
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(5);
        var accessToken = jwtIssuer.IssueToken(license, clients);

        return new ActivateLicenseResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            LicenseId: license.Id,
            ExpiresAt: accessTokenExpiry);
    }

    /// <summary>
    /// Refreshes access token using a valid refresh token.
    /// </summary>
    public async Task<RefreshAccessTokenResponse> RefreshAccessTokenAsync(
        string refreshTokenValue, CancellationToken ct)
    {
        // 1. Validate refresh token exists and is valid
        var storedRefreshToken = await refreshTokenRepo.GetByTokenAsync(refreshTokenValue, ct)
            ?? throw new InvalidOperationException(_localizer["InvalidOrExpiredRefreshToken"]);

        if (!storedRefreshToken.IsValid())
            throw new InvalidOperationException(_localizer["RefreshTokenNotValid"]);

        // 2. Load associated license
        var license = await licenseRepo.GetByIdAsync(storedRefreshToken.LicenseId, ct)
            ?? throw new InvalidOperationException(_localizer["AssociatedLicenseNotFound"]);

        if (!license.IsActive)
            throw new InvalidOperationException(_localizer["AssociatedLicenseNotActive"]);

        if (license.ExpiryDate <= DateTime.UtcNow)
            throw new InvalidOperationException(_localizer["AssociatedLicenseExpired"]);

        // 3. Generate new access token
        var accessTokenExpiry = DateTime.UtcNow.AddHours(1);
        var clients = license.LicenseClients.Select(lc => lc.Client)
                     .Where(c => c != null).ToList();
        var newAccessToken = jwtIssuer.IssueToken(license, clients);

        // 4. Revoke old refresh token and create new one (token rotation)
        storedRefreshToken.Revoke();
        storedRefreshToken.ReplacedByToken = refreshTokenValue;
        refreshTokenRepo.Update(storedRefreshToken);

        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        var newRefreshTokenValue = jwtIssuer.GenerateRefreshToken();
        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            license.Id, newRefreshTokenValue, newRefreshTokenExpiry);

        await refreshTokenRepo.AddAsync(newRefreshToken, ct);

        _logger.LogInformation(string.Format(_localizer["RefreshTokenRotated"], license.Id));

        return new RefreshAccessTokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshTokenValue,
            LicenseId: license.Id,
            ExpiresAt: accessTokenExpiry);
    }

    /// <summary>
    /// Revokes all refresh tokens for a given license.
    /// </summary>
    public async Task RevokeAllTokensAsync(Guid licenseId, CancellationToken ct)
    {
        var tokens = await refreshTokenRepo.GetByLicenseIdAsync(licenseId, ct);

        foreach (var token in tokens)
        {
            if (!token.IsRevoked)
            {
                token.Revoke();
                refreshTokenRepo.Update(token);
            }
        }

        _logger.LogInformation(string.Format(_localizer["AllTokensRevoked"], licenseId));
    }
}
