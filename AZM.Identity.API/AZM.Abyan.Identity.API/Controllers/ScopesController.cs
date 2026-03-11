using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Scope.Create;
using AZM.Abyan.Identity.Application.Commands.Scope.Delete;
using AZM.Abyan.Identity.Application.Commands.Scope.Update;
using AZM.Abyan.Identity.Application.DTOs.Scopes;
using AZM.Abyan.Identity.Application.Queries.Scope.GetScopeByName;
using AZM.Abyan.Identity.Application.Queries.Scope.GetScopes;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class ScopesController : BaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetScopes(string realm, string clientId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetScopesQuery(realm, clientId), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{scopeName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetScopeByName(string realm, string clientId, string scopeName, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetScopeByNameQuery(realm, clientId, scopeName), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateScope(string realm, string clientId, [FromBody] CreateScopeRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateScopeCommand
        {
            CreateScopeRequest = request,
            RealmName = realm,
            KeycloakClientId = clientId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{scopeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateScope(string realm, string clientId, Guid scopeId, [FromBody] UpdateScopeRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateScopeCommand
        {
            ScopeId = scopeId,
            UpdateScopeRequest = request,
            RealmName = realm,
            KeycloakClientId = clientId,
            KeycloakScopeId = scopeId.ToString()
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpDelete("{scopeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteScope(string realm, string clientId, Guid scopeId, CancellationToken cancellationToken)
    {
        var command = new DeleteScopeCommand
        {
            ScopeId = scopeId,
            RealmName = realm,
            KeycloakClientId = clientId,
            KeycloakScopeId = scopeId.ToString()
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }
}
