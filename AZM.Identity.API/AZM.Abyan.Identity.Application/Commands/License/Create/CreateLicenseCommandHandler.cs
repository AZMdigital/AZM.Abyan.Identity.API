using AZM.Abyan.Identity.Application.Commands.License.Create;
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
    IStringLocalizer<SharedResource> localizer, IEncryptionService encryptionService) : IRequestHandler<CreateLicenseCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.License, Guid> _repository = licenseRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;
    private readonly IEncryptionService _encryptionService=encryptionService;
    public async Task<Result<Guid>> Handle(CreateLicenseCommand request, CancellationToken cancellationToken)
    {
        try
        { 
            var PrivateKeyEncrypt = _encryptionService.Encrypt(request.PrivateKeyEncrypted);
            // Create new License entity
            var license = new Domain.Entities.License
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                ClientId = request.ClientId,
                LicenseKeyHash = request.LicenseKeyHash,
                PublicKey = request.PublicKey,
                PrivateKeyEncrypted = PrivateKeyEncrypt,
                IssuedAt = request.IssuedAt,
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                IsRevoked = false,
                PackageName = request.PackageName,
                Domain = request.Domain,
                ServerIps = request.ServerIps,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
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
