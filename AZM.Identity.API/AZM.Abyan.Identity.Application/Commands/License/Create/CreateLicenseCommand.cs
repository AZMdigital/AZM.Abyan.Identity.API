using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.License.Create;

public class CreateLicenseCommand : IRequest<Result<Guid>>
{
    public Guid TenantId { get; set; }
    public Guid ClientId { get; set; }
    public string LicenseKeyHash { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKeyEncrypted { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ServerIps { get; set; } = string.Empty;
}
