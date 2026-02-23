using System.Text.Json;
using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.License.GetSignedLicense;

public class GetSignedLicenseQueryHandler(
    ILicenseRepository licenseRepository,
    ILicenseService licenseService,
    IRsaKeyProvider rsaKeyProvider,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetSignedLicenseQuery, Result<LicenseFileDto>>
{
    private readonly ILicenseRepository _licenseRepository = licenseRepository;
    private readonly ILicenseService _licenseService = licenseService;
    private readonly IRsaKeyProvider _rsaKeyProvider = rsaKeyProvider;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<LicenseFileDto>> Handle(GetSignedLicenseQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var license = await _licenseRepository.GetByIdAsync(request.LicenseId, cancellationToken);
            if (license == null)
                return Result<LicenseFileDto>.NotFound(_localizer["LicenseNotFound"] ?? "License not found");

            var dto = new LicenseFileDto
            {
                LicenseId = license.Id.ToString(),
                TenantId = license.TenantId.ToString(),
                ClientName = license.Client?.Name ?? "",
                Package = license.PackageName,
                ExpiryDate = license.ExpiryDate
            };

            // Sign the license using the private key
            var jsonToSign = JsonSerializer.Serialize(dto);
            dto.Signature = _licenseService.Sign(jsonToSign, _rsaKeyProvider.GetPrivateKeyPem());

            return Result<LicenseFileDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<LicenseFileDto>.Failure(ex.Message);
        }
    }
}
