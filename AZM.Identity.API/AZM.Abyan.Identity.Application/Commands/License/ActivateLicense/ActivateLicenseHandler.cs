using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AZM.Abyan.Identity.Application.Commands.License.ActivateLicense;

public class ActivateLicenseHandler(
    ILicenseRepository licenseRepo,
    ITenantRepository tenantRepo,
    IClientRepository clientRepo,
    ILicenseService licenseService,
    IJwtIssuerService jwtIssuer,
    IRsaKeyProvider rsaKeyProvider,
    IKeycloakVerifier keycloakVerifier,
    ILogger<ActivateLicenseHandler> logger)
    : IRequestHandler<ActivateLicenseCommand, ActivateLicenseResponse>
{
    public async Task<ActivateLicenseResponse> Handle(
        ActivateLicenseCommand request, CancellationToken ct)
    {
        // 1. Parse license file
        var dto = licenseService.Parse(request.LicenseFile)
            ?? throw new InvalidOperationException("Invalid license file format.");

        // 2. Verify RSA signature against the global public key
        if (!licenseService.ValidateSignature(request.LicenseFile, rsaKeyProvider.GetPublicKeyPem()))
            throw new InvalidOperationException("License signature is invalid.");

        // 3. Verify Tenant exists in DB
        if (!Guid.TryParse(dto.TenantId, out var tenantGuid))
            throw new InvalidOperationException("Invalid Tenant ID format.");

        var tenant = await tenantRepo.GetActiveByIdAsync(tenantGuid, ct)
            ?? throw new InvalidOperationException("Tenant not found or inactive.");

        // 4. Verify Client exists in DB
           Domain.Entities.Client client = await clientRepo.GetByNameAsync(dto.ClientName, ct)
            ?? throw new InvalidOperationException($"Client '{dto.ClientName}' not found.");

        // 5. Keycloak cross-check (Tenant realm + Client)
        if (!await keycloakVerifier.TenantExistsAsync(tenant.Name, ct))
            throw new InvalidOperationException("Tenant realm not found in Keycloak.");

        if (!await keycloakVerifier.ClientExistsAsync(tenant.Name, client.Name, ct))
            throw new InvalidOperationException("Client not found in Keycloak realm.");

        if (!Guid.TryParse(dto.LicenseId, out var licenseIdGuid))
            throw new InvalidOperationException("Invalid License ID format.");

        // 6. Load or create License record
        var existing = await licenseRepo.GetByIdAsync(licenseIdGuid, ct);

        if (existing is not null)
        {
            // Tamper detection
            if (!licenseService.VerifyHash(request.LicenseFile, existing.LicenseKeyHash))
                throw new InvalidOperationException("License payload has been tampered.");

            LicenseDomainService.EnsureIsActive(existing);
            LicenseDomainService.EnsureNotExpired(existing);

            existing.Activate();
            licenseRepo.Update(existing);
            logger.LogInformation("License {Id} re-activated", existing.Id);
        }
        else
        {
            if (dto.ExpiryDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot activate an expired license.");

            var keyHash = licenseService.ComputeHash(request.LicenseFile);

            existing = Domain.Entities.License.Create(
                licenseIdGuid, tenant.Id, client.Id,
                keyHash, dto.Package, dto.ExpiryDate);


            existing.Activate();
            await licenseRepo.AddAsync(existing, ct);
            logger.LogInformation("License {Id} created for tenant {TenantId}", existing.Id, tenant.Id);
        }

        await licenseRepo.SaveChangesAsync(ct);

        // 7. Issue RS256 JWT
        var token = jwtIssuer.IssueToken(existing, client);
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        return new ActivateLicenseResponse(token, existing.Id, expiresAt);
    }
}
