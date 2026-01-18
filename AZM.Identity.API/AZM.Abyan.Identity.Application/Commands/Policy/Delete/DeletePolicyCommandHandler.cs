using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Policy.Delete;

public class DeletePolicyCommandHandler(
    IRepository<Domain.Entities.Policy, Guid> policyRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeletePolicyCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Policy, Guid> _policyRepository = policyRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(DeletePolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
            if (policy == null)
            {
                return Result<bool>.NotFound(_localizer["PolicyNotFound"] ?? "Policy not found");
            }

            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Delete policy in Keycloak first
            await _keycloakService.DeletePolicyAsync(
                request.RealmName,
                request.KeycloakClientId,
                request.KeycloakPolicyId,
                adminToken,
                cancellationToken);

            // Soft delete policy in database
            policy.SoftDelete();
            _policyRepository.Update(policy);
            await _policyRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Deleted(true, _localizer["PolicyDeletedSuccessfully"] ?? "Policy deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
