using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Domain.Services;
using MediatR;
using Microsoft.Extensions.Localization;
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
    ILogger<ActivateLicenseHandler> logger,
    TokenService tokenService,
    IStringLocalizer<SharedResource> localizer)
    : IRequestHandler<ActivateLicenseCommand, ActivateLicenseResponse>
{
    public async Task<ActivateLicenseResponse> Handle(
        ActivateLicenseCommand request, CancellationToken ct)
    {
        // 1. Parse license file
        var dto = licenseService.Parse(request.LicenseFile)
            ?? throw new InvalidOperationException(localizer["InvalidLicenseFileFormat"]);

        // 2. Verify RSA signature against the global public key
        //if (!licenseService.ValidateSignature(request.LicenseFile, rsaKeyProvider.GetPublicKeyPem()))
        //    throw new InvalidOperationException(localizer["LicenseSignatureInvalid"]);

        // 3. Verify Tenant exists in DB
        if (!Guid.TryParse(dto.TenantId, out var tenantGuid))
            throw new InvalidOperationException(localizer["InvalidTenantIdFormat"]);

        var tenant = await tenantRepo.GetActiveByIdAsync(tenantGuid, ct)
            ?? throw new InvalidOperationException(localizer["TenantNotFoundOrInactive"]);

        // 4. Verify all clients exist in DB and Keycloak
        var clients = new List<Domain.Entities.Client>();
        foreach (var clientName in dto.ClientNames)
        {
            var client = await clientRepo.GetByNameAsync(clientName, ct)
                ?? throw new InvalidOperationException(string.Format(localizer["ClientNotFound"], clientName));
            
            // Keycloak cross-check for this client
            //if (!await keycloakVerifier.ClientExistsAsync(tenant.Name, client.Name, ct))
            //    throw new InvalidOperationException(localizer["ClientNotFoundInRealm"]);
            
            clients.Add(client);
        }

        // 5. Keycloak cross-check (Tenant realm)
        //if (!await keycloakVerifier.TenantExistsAsync(tenant.Name, ct))
        //    throw new InvalidOperationException(localizer["TenantRealmNotFound"]);

        if (!Guid.TryParse(dto.LicenseId, out var licenseIdGuid))
            throw new InvalidOperationException(localizer["InvalidLicenseIdFormat"]);

        // 6. Load or create License record
        var existing = await licenseRepo.GetByIdAsync(licenseIdGuid, ct);

        if (existing is not null)
        {
            // Tamper detection
            //if (!licenseService.VerifyHash(request.LicenseFile, existing.LicenseKeyHash))
            //    throw new InvalidOperationException(localizer["LicensePayloadTampered"]);

            LicenseDomainService.EnsureIsActive(existing);
            LicenseDomainService.EnsureNotExpired(existing);

            existing.Activate();
            licenseRepo.Update(existing);
            logger.LogInformation(string.Format(localizer["LicenseReactivated"], existing.Id));
        }
        else
        {
            if (dto.ExpiryDate <= DateTime.UtcNow)
                throw new InvalidOperationException(localizer["CannotActivateExpiredLicense"]);

            var keyHash = licenseService.ComputeHash(request.LicenseFile);

            existing = Domain.Entities.License.Create(
                licenseIdGuid, tenant.Id,
                keyHash, dto.Package, dto.ExpiryDate);

            // Add all clients to the license
            foreach (var client in clients)
            {
                existing.AddClient(client.Id);
            }

            existing.Activate();
            await licenseRepo.AddAsync(existing, ct);
            logger.LogInformation(string.Format(localizer["LicenseCreated"], existing.Id, tenant.Id));
        }

        await licenseRepo.SaveChangesAsync(ct);

        // 7. Issue RS256 JWT (use first client for token)
        var primaryClient = clients.First();
        var response = await tokenService.ActivateLicenseAsync(existing, clients, ct);

        return new ActivateLicenseResponse(response.AccessToken, response.RefreshToken, response.LicenseId, response.ExpiresAt);
    }
}
