using System.Text.Json;
using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.License.Update;

public class UpdateLicenseCommandHandler(
    ILicenseRepository licenseRepository,
    ILicenseService licenseService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateLicenseCommand, Result<Guid>>
{
    private readonly ILicenseRepository _licenseRepository = licenseRepository;
    private readonly ILicenseService _licenseService = licenseService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(UpdateLicenseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // GetByIdAsync in LicenseRepository includes Tenant and Client
            var license = await _licenseRepository.GetByIdAsync(request.LicenseId, cancellationToken);

            if (license == null)
            {
                return Result<Guid>.NotFound(_localizer["LicenseNotFound"]);
            }

            bool needsHashRecalculation = false;

            // Update only provided fields
            if (request.ExpiryDate.HasValue && request.ExpiryDate.Value != license.ExpiryDate)
            {
                license.ExpiryDate = request.ExpiryDate.Value;
                needsHashRecalculation = true;
            }

            if (request.MaxUsers.HasValue)
                license.MaxUsers = request.MaxUsers.Value;

            if (request.IsActive.HasValue)
                license.IsActive = request.IsActive.Value;

            if (!string.IsNullOrEmpty(request.Domain))
                license.Domain = request.Domain;

            if (!string.IsNullOrEmpty(request.ServerIps))
                license.ServerIps = request.ServerIps;

            if (needsHashRecalculation)
            {
                var clientNames = license.LicenseClients?
                    .Select(lc => lc.Client?.Name ?? "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList() ?? [];

                var dto = new LicenseFileDto
                {
                    LicenseId = license.Id.ToString(),
                    TenantId = license.TenantId.ToString(),
                    ClientNames = clientNames,
                    Package = license.PackageName,
                    ExpiryDate = license.ExpiryDate
                };

                var rawJson = JsonSerializer.Serialize(dto);
                license.LicenseKeyHash = _licenseService.ComputeHash(rawJson);
            }

            license.UpdatedAt = DateTime.UtcNow;
            license.UpdatedBy = Guid.Empty;

            _licenseRepository.Update(license);
            await _licenseRepository.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Updated(license.Id, _localizer["LicenseUpdatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
