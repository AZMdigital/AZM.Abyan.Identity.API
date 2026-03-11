using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Policy.Create;
using AZM.Abyan.Identity.Application.Commands.Policy.Delete;
using AZM.Abyan.Identity.Application.Commands.Policy.Update;
using AZM.Abyan.Identity.Application.DTOs.Policies;
using AZM.Abyan.Identity.Application.Queries.Policy.GetPolicies;
using AZM.Abyan.Identity.Application.Queries.Policy.GetPolicyByName;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class PoliciesController : BaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetPolicies(string realm, string clientId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPoliciesQuery(realm, clientId), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{policyName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPolicyByName(string realm, string clientId, string policyName, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPolicyByNameQuery(realm, clientId, policyName), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreatePolicy(string realm, string clientId, [FromBody] CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePolicyCommand
        {
            CreatePolicyRequest = request,
            RealmName = realm,
            KeycloakClientId = clientId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{policyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePolicy(string realm, string clientId, Guid policyId, [FromBody] UpdatePolicyRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePolicyCommand
        {
            PolicyId = policyId,
            UpdatePolicyRequest = request,
            RealmName = realm,
            KeycloakClientId = clientId,
            KeycloakPolicyId = policyId.ToString()
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{policyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePolicy(string realm, string clientId, Guid policyId, CancellationToken cancellationToken)
    {
        var command = new DeletePolicyCommand
        {
            PolicyId = policyId,
            RealmName = realm,
            KeycloakClientId = clientId,
            KeycloakPolicyId = policyId.ToString()
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
