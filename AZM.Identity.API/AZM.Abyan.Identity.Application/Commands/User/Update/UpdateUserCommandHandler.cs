using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.User.Update;

public class UpdateUserCommandHandler(
    IRepository<Domain.Entities.User, Guid> repository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdateUserCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.User, Guid> _repository = repository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // UserId from request is the Keycloak ID which is the same as entity Id
            if (!Guid.TryParse(request.UpdateUserRequest.UserId, out var userId))
            {
                return Result<bool>.NotFound(_localizer["InvalidUserId"] ?? "Invalid user ID format");
            }

            var user = await _repository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<bool>.NotFound(_localizer["UserNotFound"] ?? "User not found");
            }

            // Update user in Keycloak first
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            await _keycloakService.UpdateUserAsync(request.UpdateUserRequest.UserId, request.UpdateUserRequest, adminToken, cancellationToken);

            // Update user in database
            user.Firstname = request.UpdateUserRequest.FirstName;
            user.Lastname = request.UpdateUserRequest.LastName;
            user.Email = request.UpdateUserRequest.Email;
            user.UpdatedAt = DateTime.UtcNow;

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Updated(true, _localizer["UserUpdatedSuccessfully"] ?? "User updated successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
