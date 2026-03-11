using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Create;
using AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Disable;
using AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/client-scopes/{clientScopeName}/protocol-mappers")]
public class ProtocolMappersController : BaseController
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateProtocolMapper(
        string realm,
        string clientScopeName,
        string clientId,
        [FromBody] CreateProtocolMapperRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProtocolMapperCommand(realm, clientScopeName, clientId, request);
        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return CreatedAtAction(nameof(CreateProtocolMapper), new { realm, clientScopeName, mapperId = result?.Data?.Id }, result?.Data);
    }

    [HttpPut("{mapperId}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DisableProtocolMapper(
        string realm,
        string clientScopeName, // This usually comes from the route, but Keycloak API might expect scope ID
        // The original code uses `clientScopeId` as parameter but route has `{clientScopeName}`.
        // Assuming clientScopeName is actually the ID or can be used as such in Keycloak
        string mapperId,
        CancellationToken cancellationToken)
    {
        // Using clientScopeName from route as clientScopeId parameter
        var command = new DisableProtocolMapperCommand(realm, clientScopeName, mapperId);
        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }
}
