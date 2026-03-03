using System.Text.Json;
using AZM.Abyan.Identity.Application.Commands.License.Create;
using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.License.Create;

public class CreateLicenseCommandHandler(
    IRepository<Domain.Entities.License, Guid> licenseRepository,
    IRepository<Domain.Entities.Tenant, Guid> tenantRepository,
    IRepository<Domain.Entities.Client, Guid> clientRepository,
    ILicenseService licenseService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateLicenseCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.License, Guid> _repository = licenseRepository;
    private readonly IRepository<Domain.Entities.Tenant, Guid> _tenantRepository = tenantRepository;
    private readonly IRepository<Domain.Entities.Client, Guid> _clientRepository = clientRepository;
    private readonly ILicenseService _licenseService = licenseService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateLicenseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null) return Result<Guid>.NotFound(_localizer["TenantNotFound"] ?? "Tenant not found");

            var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null) return Result<Guid>.NotFound(_localizer["ClientNotFound"] ?? "Client not found");

            var licenseId = Guid.NewGuid();

            // Assemble a DTO that represents the "source of truth" for the hash
            var dto = new LicenseFileDto
            {
                LicenseId = licenseId.ToString(),
                TenantId = tenant.Id.ToString(),
                ClientName = client.Name ?? "",
                Package = request.PackageName,
                ExpiryDate = request.ExpiryDate
            };

            // Convert to JSON and Compute Hash (same logic as in activation)
            var rawJson = JsonSerializer.Serialize(dto);
            var dynamicHash = _licenseService.ComputeHash(rawJson);

            // Create new License entity
            var license = new Domain.Entities.License
            {
                Id = licenseId,
                TenantId = request.TenantId,
                ClientId = request.ClientId,
                LicenseKeyHash = dynamicHash,
                IssuedAt = DateTime.UtcNow,
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                PackageName = request.PackageName,
                Domain = request.Domain,
                ServerIps = request.ServerIps,
                IsActive = false
            };

            await _repository.CreateAsync(license, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Created(license.Id, _localizer["LicenseCreatedSuccessfully"] ?? "License created successfully");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
