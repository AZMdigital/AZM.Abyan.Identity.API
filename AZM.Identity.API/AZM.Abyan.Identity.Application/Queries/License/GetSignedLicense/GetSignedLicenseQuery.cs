using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.License.GetSignedLicense;

public class GetSignedLicenseQuery : IRequest<Result<LicenseFileDto>>
{
    public Guid LicenseId { get; set; }

    public GetSignedLicenseQuery(Guid licenseId)
    {
        LicenseId = licenseId;
    }
}
