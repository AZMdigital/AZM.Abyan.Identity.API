using AZM.Abyan.Identity.Application.DTOs.Licenses;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using Mapster;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.License.GetLicenseById;

public class GetLicenseByIdQueryHandler(
    IRepository<Domain.Entities.License, Guid> licenseRepository,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetLicenseByIdQuery, Result<LicenseResponse>>
{
    private readonly IRepository<Domain.Entities.License, Guid> _repository = licenseRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<LicenseResponse>> Handle(GetLicenseByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var license = await _repository.GetByIdAsync(request.LicenseId, cancellationToken);

            if (license == null)
            {
                return Result<LicenseResponse>.NotFound(_localizer["LicenseNotFound"] ?? "License not found");
            }

            var response = license.Adapt<LicenseResponse>();
            return Result<LicenseResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<LicenseResponse>.Failure(ex.Message);
        }
    }
}
