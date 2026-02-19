using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.License.Update;

public class UpdateLicenseCommandHandler(
    IRepository<Domain.Entities.License, Guid> licenseRepository,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateLicenseCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.License, Guid> _repository = licenseRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(UpdateLicenseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var license = await _repository.GetByIdAsync(request.LicenseId, cancellationToken);

            if (license == null)
            {
                return Result<Guid>.NotFound(_localizer["LicenseNotFound"] ?? "License not found");
            }

            // Update only provided fields
            if (request.ExpiryDate.HasValue)
                license.ExpiryDate = request.ExpiryDate.Value;

            if (request.MaxUsers.HasValue)
                license.MaxUsers = request.MaxUsers.Value;

            if (request.IsRevoked.HasValue)
                license.IsRevoked = request.IsRevoked.Value;

            if (!string.IsNullOrEmpty(request.Domain))
                license.Domain = request.Domain;

            if (!string.IsNullOrEmpty(request.ServerIps))
                license.ServerIps = request.ServerIps;

            license.UpdatedAt = DateTime.UtcNow;
            license.UpdatedBy = Guid.Empty;

             _repository.Update(license);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Updated(license.Id, _localizer["LicenseUpdatedSuccessfully"] ?? "License updated successfully");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
