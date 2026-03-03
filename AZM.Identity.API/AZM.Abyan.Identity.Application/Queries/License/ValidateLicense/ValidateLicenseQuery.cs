using System;
using AZM.Abyan.Identity.Application.DTOs;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.License.ValidateLicense;

public record ValidateLicenseQuery(Guid LicenseId, string? CurrentDomain, string? CurrentIp)
    : IRequest<ValidateLicenseResponse>;
