using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.License.Delete;

public class DeleteLicenseCommand(Guid licenseId) : IRequest<Result<Guid>>
{
    public Guid LicenseId { get; set; } = licenseId;
}
