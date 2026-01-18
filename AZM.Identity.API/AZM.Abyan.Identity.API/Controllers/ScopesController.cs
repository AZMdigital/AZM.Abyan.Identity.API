using AZM.Abyan.Identity.Application.Commands.Scope.Create;
using AZM.Abyan.Identity.Application.Commands.Scope.Delete;
using AZM.Abyan.Identity.Application.Commands.Scope.Update;
using AZM.Abyan.Identity.Application.DTOs.Scopes;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class ScopesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IKeycloakService _keycloakService;

    public ScopesController(IMediator mediator, IKeycloakService keycloakService)
    {
        _mediator = mediator;
        _keycloakService = keycloakService;
    }

    [HttpGet]
    public async Task<ActionResult> GetScopes(string realm, string clientId, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var scopes = await _keycloakService.GetAllScopesAsync(realm, clientId, adminToken, cancellationToken);
            return Ok(scopes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{scopeName}")]
    public async Task<ActionResult> GetScopeByName(string realm, string clientId, string scopeName, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var scope = await _keycloakService.GetScopeAsync(realm, clientId, scopeName, adminToken, cancellationToken);
            
            if (scope == null)
                return NotFound(new { message = $"Scope with name {scopeName} not found" });

            return Ok(scope);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateScope(string realm, string clientId, [FromBody] CreateScopeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateScopeCommand
            {
                CreateScopeRequest = request,
                RealmName = realm,
                KeycloakClientId = clientId
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{scopeId}")]
    public async Task<ActionResult> UpdateScope(string realm, string clientId, string scopeId, [FromBody] UpdateScopeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(scopeId, out var scopeIdGuid))
            {
                return BadRequest(new { message = "Invalid scope ID format" });
            }

            // Get Keycloak scope ID - you may need to store this mapping or fetch it
            // For now, assuming scopeId parameter is the Keycloak scope ID (string)
            var command = new UpdateScopeCommand
            {
                ScopeId = scopeIdGuid,
                UpdateScopeRequest = request,
                RealmName = realm,
                KeycloakClientId = clientId,
                KeycloakScopeId = scopeId // Assuming scopeId is Keycloak scope ID
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{scopeId}")]
    public async Task<ActionResult> DeleteScope(string realm, string clientId, string scopeId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(scopeId, out var scopeIdGuid))
            {
                return BadRequest(new { message = "Invalid scope ID format" });
            }

            // Get Keycloak scope ID - you may need to store this mapping or fetch it
            var command = new DeleteScopeCommand
            {
                ScopeId = scopeIdGuid,
                RealmName = realm,
                KeycloakClientId = clientId,
                KeycloakScopeId = scopeId // Assuming scopeId is Keycloak scope ID
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
