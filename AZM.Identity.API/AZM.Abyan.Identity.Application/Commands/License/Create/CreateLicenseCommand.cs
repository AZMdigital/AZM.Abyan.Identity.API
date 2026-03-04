using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.License.Create;

public record CreateLicenseCommand : IRequest<Result<LicenseFileDto>>
{
    public Guid TenantId { get; set; }
    public List<string> ClientNames { get; set; } = new();
    public DateTime ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ServerIps { get; set; } = string.Empty;
}
