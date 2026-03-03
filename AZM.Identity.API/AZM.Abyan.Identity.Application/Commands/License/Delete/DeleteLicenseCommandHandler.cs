using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.License.Delete;

public class DeleteLicenseCommandHandler(
    IRepository<Domain.Entities.License, Guid> licenseRepository,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeleteLicenseCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.License, Guid> _repository = licenseRepository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(DeleteLicenseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var license = await _repository.GetByIdAsync(request.LicenseId, cancellationToken);

            if (license == null)
            {
                return Result<Guid>.NotFound(_localizer["LicenseNotFound"] ?? "License not found");
            }

            await _repository.DeleteAsync(license.Id, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Deleted(license.Id, _localizer["LicenseDeletedSuccessfully"] ?? "License deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
