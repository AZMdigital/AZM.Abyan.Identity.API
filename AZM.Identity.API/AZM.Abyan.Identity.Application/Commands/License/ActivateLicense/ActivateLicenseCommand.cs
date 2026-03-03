using AZM.Abyan.Identity.Application.DTOs;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.License.ActivateLicense;

public record ActivateLicenseCommand(string LicenseFile) : IRequest<ActivateLicenseResponse>;
