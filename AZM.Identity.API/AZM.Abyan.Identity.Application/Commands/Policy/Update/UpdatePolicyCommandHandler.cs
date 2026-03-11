using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Policy.Update;

public class UpdatePolicyCommandHandler(
    IRepository<Domain.Entities.Policy, Guid> policyRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<UpdatePolicyCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Policy, Guid> _policyRepository = policyRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
            if (policy == null)
            {
                return Result<bool>.NotFound(_localizer["PolicyNotFound"]);
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Create PolicyDto for Keycloak
            var policyDto = new PolicyDto
            {
                Id = request.KeycloakPolicyId,
                Name = request.UpdatePolicyRequest.Name,
                Type = request.UpdatePolicyRequest.Type,
                Logic = request.UpdatePolicyRequest.Logic,
                DecisionStrategy = request.UpdatePolicyRequest.DecisionStrategy,
                Config = request.UpdatePolicyRequest.Config
            };

            // Update policy in Keycloak first
            await _keycloakService.UpdatePolicyAsync(
                request.RealmName,
                request.KeycloakClientId,
                request.KeycloakPolicyId,
                policyDto,
                adminToken,
                cancellationToken);

            // Update policy in database
            policy.Name = request.UpdatePolicyRequest.Name;
            policy.UpdatedAt = DateTime.UtcNow;
            _policyRepository.Update(policy);
            await _policyRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Updated(true, _localizer["PolicyUpdatedSuccessfully"]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
