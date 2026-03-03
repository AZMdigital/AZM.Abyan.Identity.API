using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.License.Delete;

public class DeleteLicenseCommand : IRequest<Result<Guid>>
{
    public Guid LicenseId { get; set; }

    public DeleteLicenseCommand(Guid licenseId)
    {
        LicenseId = licenseId;
    }
}
