using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.License.GetLicenseById;

public class GetLicenseByIdQuery : IRequest<Result<DTOs.Licenses.LicenseResponse>>
{
    public Guid LicenseId { get; set; }

    public GetLicenseByIdQuery(Guid licenseId)
    {
        LicenseId = licenseId;
    }
}
