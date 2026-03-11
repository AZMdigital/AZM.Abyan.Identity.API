using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
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
            // Check if OrganizationName is null or empty
            if (string.IsNullOrEmpty(request.OrganizationName))
            {
                return Result<Guid>.Failure(_localizer["OrganizationNameRequired"]);
            }

            // Resolve RealmId from RealmName if provided
            Guid? realmId = null;
            Guid? tenantId = null;
            // Resolve RealmId from RealmName
            var resolvedRealmId = await _realmResolverService.ResolveRealmIdAsync(request.OrganizationName, cancellationToken);
            if (!resolvedRealmId.HasValue)
            {
                return Result<Guid>.Failure(_localizer["TenantNotFound"]);
            }
             realmId = resolvedRealmId.Value;
             tenantId = resolvedRealmId.Value; // Keep TenantId for backward compatibility

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
                Enabled = true,
                EmailVerified = true,
                OrganizationName = request.OrganizationName
            };

            var keycloakUserIdString = await _keycloakService.CreateUserAsync(createUserRequest, adminToken, cancellationToken);

            if (string.IsNullOrEmpty(keycloakUserIdString) || !Guid.TryParse(keycloakUserIdString, out var keycloakUserId))
            {
                return Result<Guid>.Failure(_localizer["FailedToCreateUserInKeycloak"]);
            }

            // Create local entity with ID from Keycloak
            var user = new Domain.Entities.User
            {
                Id = keycloakUserId,
                Username = request.Username,
                Email = request.Email,
                Firstname = request.FirstName,
                Lastname = request.LastName,
                TenantId = tenantId, // Keep for backward compatibility
                                     // RealmId = realmId, // Add RealmId similar to Client
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _userRepository.CreateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(user.Id, _localizer["UserCreatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

