using AZM.Abyan.Identity.Application.Commands.Policy.Create;
using AZM.Abyan.Identity.Application.Commands.Policy.Delete;
using AZM.Abyan.Identity.Application.Commands.Policy.Update;
using AZM.Abyan.Identity.Application.DTOs.Policies;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class PoliciesController(IMediator mediator, IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    [HttpGet]
    public async Task<ActionResult> GetPolicies(string realm, string clientId, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var policies = await _keycloakService.GetAllPoliciesAsync(realm, clientId, adminToken, cancellationToken);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpGet("{policyName}")]
    public async Task<ActionResult> GetPolicyByName(string realm, string clientId, string policyName, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var policy = await _keycloakService.GetPolicyAsync(realm, clientId, policyName, adminToken, cancellationToken);
            
            if (policy == null)
                return NotFound(new { message = _localizer["PolicyNotFound"] });

            return Ok(policy);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreatePolicy(string realm, string clientId, [FromBody] CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreatePolicyCommand
            {
                CreatePolicyRequest = request,
                RealmName = realm,
                KeycloakClientId = clientId
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("{policyId}")]
    public async Task<ActionResult> UpdatePolicy(string realm, string clientId, string policyId, [FromBody] UpdatePolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(policyId, out var policyIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidPolicyId"] });
            }

            // Get Keycloak policy ID - you may need to store this mapping or fetch it
            var command = new UpdatePolicyCommand
            {
                PolicyId = policyIdGuid,
                UpdatePolicyRequest = request,
                RealmName = realm,
                KeycloakClientId = clientId,
                KeycloakPolicyId = policyId // Assuming policyId is Keycloak policy ID (string)
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("{policyId}")]
    public async Task<ActionResult> DeletePolicy(string realm, string clientId, string policyId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(policyId, out var policyIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidPolicyId"] });
            }

            // Get Keycloak policy ID - you may need to store this mapping or fetch it
            var command = new DeletePolicyCommand
            {
                PolicyId = policyIdGuid,
                RealmName = realm,
                KeycloakClientId = clientId,
                KeycloakPolicyId = policyId // Assuming policyId is Keycloak policy ID (string)
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }
}
