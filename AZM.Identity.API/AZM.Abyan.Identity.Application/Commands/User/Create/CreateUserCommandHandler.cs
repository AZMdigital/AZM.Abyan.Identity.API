using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.User.Create;

public class CreateUserCommandHandler(
    IRepository<Domain.Entities.User, Guid> userRepository,
    IKeycloakService keycloakService,
    IRealmResolverService realmResolverService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.User, Guid> _userRepository = userRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IRealmResolverService _realmResolverService = realmResolverService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Resolve TenantId from RealmName if provided
            Guid? tenantId = null;
            if (!string.IsNullOrWhiteSpace(request.RealmName))
            {
                var resolvedTenantId = await _realmResolverService.ResolveRealmIdAsync(request.RealmName, cancellationToken);
                if (!resolvedTenantId.HasValue)
                {
                    return Result<Guid>.Failure(_localizer["TenantNotFound"] ?? $"Tenant/Realm '{request.RealmName}' not found");
                }
                tenantId = resolvedTenantId.Value;
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Create user in Keycloak first
            // Note: KeycloakService.CreateUserAsync uses default realm from config
            // If realm is provided in command, we might need to update KeycloakService to accept realm parameter
            var createUserRequest = new CreateUserRequest
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                Enabled = request.Enabled,
                EmailVerified = request.EmailVerified
            };

            var keycloakUserIdString = await _keycloakService.CreateUserAsync(createUserRequest, adminToken, cancellationToken);

            if (string.IsNullOrEmpty(keycloakUserIdString) || !Guid.TryParse(keycloakUserIdString, out var keycloakUserId))
            {
                return Result<Guid>.Failure(_localizer["FailedToCreateUserInKeycloak"] ?? "Failed to create user in Keycloak");
            }

            // Create local entity with ID from Keycloak
            var user = new Domain.Entities.User
            {
                Id = keycloakUserId,
                Username = request.Username,
                Email = request.Email,
                Firstname = request.FirstName,
                Lastname = request.LastName,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _userRepository.CreateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(user.Id, _localizer["UserCreatedSuccessfully"] ?? "User created successfully");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

