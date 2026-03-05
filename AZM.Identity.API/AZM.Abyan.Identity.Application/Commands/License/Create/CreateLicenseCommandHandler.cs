using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Licenses;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace AZM.Abyan.Identity.Application.Commands.License.Create;

public class CreateLicenseCommandHandler(
    IRepository<Domain.Entities.License, Guid> licenseRepository,
    IRepository<Domain.Entities.LicenseClient, (Guid, Guid)> licenseClientRepository,
    IRepository<Domain.Entities.Tenant, Guid> tenantRepository,
    IRepository<Domain.Entities.Client, Guid> clientRepository,
    ILicenseService licenseService,
    IRsaKeyProvider rsaKeyProvider,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateLicenseCommand, Result<LicenseFileDto>>
{
    private readonly IRepository<Domain.Entities.License, Guid> _licenseRepository = licenseRepository;
    private readonly IRepository<Domain.Entities.LicenseClient, (Guid, Guid)> _licenseClientRepository = licenseClientRepository;
    private readonly IRepository<Domain.Entities.Tenant, Guid> _tenantRepository = tenantRepository;
    private readonly IRepository<Domain.Entities.Client, Guid> _clientRepository = clientRepository;
    private readonly ILicenseService _licenseService = licenseService;
    private readonly IRsaKeyProvider _rsaKeyProvider = rsaKeyProvider;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<LicenseFileDto>> Handle(CreateLicenseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null)
                return Result<LicenseFileDto>.NotFound(_localizer["TenantNotFound"] ?? "Tenant not found");

            // Validate client names provided
            if (!request.ClientNames.Any())
                return Result<LicenseFileDto>.Failure(_localizer["AtLeastOneClientRequired"] ?? "At least one client is required");

            // Resolve client names to client entities from database
            var clients = new List<Domain.Entities.Client>();
            var clientIds = new List<Guid>();
            
            foreach (var clientName in request.ClientNames)
            {
                // Get all clients and find by name - use ToList() first for client-side evaluation
                var allClients = _clientRepository.GetWhere(null).ToList();
                var client = allClients.FirstOrDefault(c =>     
                    c.Name.Equals(clientName, StringComparison.OrdinalIgnoreCase) &&
                    c.RealmId == request.TenantId);
                
                if (client == null)
                    return Result<LicenseFileDto>.NotFound($"{_localizer["ClientNotFound"] ?? "Client"} '{clientName}'");
                
                clients.Add(client);
                clientIds.Add(client.Id);
            }

            var licenseId = Guid.NewGuid();

            // Assemble a DTO that represents the "source of truth" for the hash
            // Include all client names in the list
            var dto = new LicenseFileDto
            {
                LicenseId = licenseId.ToString(),
                TenantId = tenant.Id.ToString(),
                ClientNames = clients.Select(c => c.Name ?? "").ToList(),
                Package = request.PackageName,
                ExpiryDate = request.ExpiryDate
            };

            // Convert to JSON and Compute Hash (same logic as in activation)
            var rawJson = JsonSerializer.Serialize(dto);
            var dynamicHash = _licenseService.ComputeHash(rawJson);

            // Sign the license using the private key
            dto.Signature = _licenseService.Sign(rawJson, _rsaKeyProvider.GetPrivateKeyPem());

            // Create new License entity (without single ClientId)
            var license = new Domain.Entities.License
            {
                Id = licenseId,
                TenantId = request.TenantId,
                LicenseKeyHash = dynamicHash,
                IssuedAt = DateTime.UtcNow,
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                PackageName = request.PackageName,
                Domain = request.Domain,
                ServerIps = request.ServerIps,
                IsActive = true
            };

            // Save the license first
            await _licenseRepository.CreateAsync(license, cancellationToken);
            await _licenseRepository.SaveChangesAsync(cancellationToken);

            // Create LicenseClient associations for each client
            foreach (var clientId in clientIds)
            {
                var licenseClient = new Domain.Entities.LicenseClient
                {
                    LicenseId = licenseId,
                    ClientId = clientId,
                    CreatedAt = DateTime.UtcNow
                };

                await _licenseClientRepository.CreateAsync(licenseClient, cancellationToken);
            }

            await _licenseClientRepository.SaveChangesAsync(cancellationToken);

            return Result<LicenseFileDto>.Created(dto, 
                _localizer["LicenseCreatedSuccessfully"] ?? "License created successfully");
        }
        catch (Exception ex)
        {
            return Result<LicenseFileDto>.Failure(ex.Message);
        }
    }
}
