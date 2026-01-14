using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Policy.Create;

public class CreatePolicyCommandHandler(
    IRepository<Domain.Entities.Policy, Guid> policyRepository,
    IRepository<Domain.Entities.Role, Guid> roleRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<CreatePolicyCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.Policy, Guid> _policyRepository = policyRepository;
    private readonly IRepository<Domain.Entities.Role, Guid> _roleRepository = roleRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            string keycloakPolicyId = string.Empty;

            //// For other policy types, create using PolicyDto
            //var policyDto = new PolicyDto
            //{
            //    Name = request.CreatePolicyRequest.Name,
            //    Type = "role",
            //    Logic = "POSITIVE",
            //    DecisionStrategy = "UNANIMOUS",
            //    Config = request.CreatePolicyRequest.Config
            //};

            // Note: We need a generic CreatePolicyAsync method, but for now use role policy
            // This would need to be extended based on policy type
            if (request.CreatePolicyRequest.RoleNames.Any())
            {
                keycloakPolicyId = await _keycloakService.CreateRolePolicyAsync(
                    request.RealmName,
                    request.KeycloakClientId,
                    request.CreatePolicyRequest.Name,
                    request.CreatePolicyRequest.RoleNames,
                    adminToken,
                    cancellationToken);
            }


            if (string.IsNullOrEmpty(keycloakPolicyId))
            {
                return Result<Guid>.Failure(_localizer["FailedToCreatePolicyInKeycloak"] ?? "Failed to create policy in Keycloak");
            }

            // Get role for database (assuming first role)
            Guid roleId = Guid.Empty;
            if (request.CreatePolicyRequest.RoleNames.Any())
            {
                var roleName = request.CreatePolicyRequest.RoleNames.First();
                var existingRole = await _roleRepository.GetWhere(r => r.Name == roleName)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingRole != null)
                {
                    roleId = existingRole.Id;
                }
            }

            // Create local entity
            var policy = new Domain.Entities.Policy
            {
                Id = Guid.Parse(keycloakPolicyId), // Keycloak policy IDs are strings, so we generate our own
                Name = request.CreatePolicyRequest.Name,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _policyRepository.CreateAsync(policy, cancellationToken);
            await _policyRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(policy.Id, _localizer["PolicyCreatedSuccessfully"] ?? "Policy created successfully");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
