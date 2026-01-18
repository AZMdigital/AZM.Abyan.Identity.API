using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.User.Delete;

public class DeleteUserCommandHandler(
    IRepository<Domain.Entities.User, Guid> repository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.User, Guid> _repository = repository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<bool>.NotFound(_localizer["UserNotFound"] ?? "User not found");
            }

            // Delete user in Keycloak first
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            await _keycloakService.DeleteUserAsync(request.UserId.ToString(), adminToken, cancellationToken);

            // Soft delete user in database
            user.SoftDelete();
            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Deleted(true, _localizer["UserDeletedSuccessfully"] ?? "User deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
