using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.License.Update;

public class UpdateLicenseCommand : IRequest<Result<Guid>>
{
    public Guid LicenseId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public bool? IsActive { get; set; }
    public string? Domain { get; set; }
    public string? ServerIps { get; set; }
}
