using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.License.ValidateLicense;

public class ValidateLicenseHandler(ILicenseRepository licenseRepo)
    : IRequestHandler<ValidateLicenseQuery, ValidateLicenseResponse>
{
    public async Task<ValidateLicenseResponse> Handle(
        ValidateLicenseQuery request, CancellationToken ct)
    {
        var license = await licenseRepo.GetByIdAsync(request.LicenseId, ct);

        if (license is null)
            return new ValidateLicenseResponse(false, "License not found.");
        if (!license.IsActive)
            return new ValidateLicenseResponse(false, "License is not active or has been revoked.");
        if (license.ExpiryDate <= DateTime.UtcNow)
            return new ValidateLicenseResponse(false, "License expired.");
        if (!license.IsDomainAllowed(request.CurrentDomain))
            return new ValidateLicenseResponse(false, "Domain mismatch.");
        if (!license.IsIpAllowed(request.CurrentIp))
            return new ValidateLicenseResponse(false, "IP mismatch.");

        return new ValidateLicenseResponse(true);
    }
}
