using AZM.Abyan.Identity.Application.DTOs.Licenses;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using Mapster;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.License.GetAllLicenses;

public class GetAllLicensesQueryHandler(
    IRepository<Domain.Entities.License, Guid> licenseRepository,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetAllLicensesQuery, Result<List<LicenseResponse>>>
{
    private readonly IRepository<Domain.Entities.License, Guid> _repository = licenseRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<List<LicenseResponse>>> Handle(GetAllLicensesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var licenses = _repository.GetWhere(null);

            var responses = licenses.Adapt<List<LicenseResponse>>();
            return Result<List<LicenseResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            return Result<List<LicenseResponse>>.Failure(ex.Message);
        }
    }
}
