using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Resource.Delete;

public class DeleteResourceCommandHandler(
    IRepository<Domain.Entities.Resource, Guid> resourceRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeleteResourceCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Resource, Guid> _resourceRepository = resourceRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
            if (resource == null)
            {
                return Result<bool>.NotFound(_localizer["ResourceNotFound"] ?? "Resource not found");
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Delete resource in Keycloak first
            await _keycloakService.DeleteResourceAsync(
                request.RealmName,
                request.KeycloakClientId,
                request.ResourceId,
                adminToken,
                cancellationToken);

            // Soft delete resource in database
            resource.SoftDelete();
            _resourceRepository.Update(resource);
            await _resourceRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Deleted(true, _localizer["ResourceDeletedSuccessfully"] ?? "Resource deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
