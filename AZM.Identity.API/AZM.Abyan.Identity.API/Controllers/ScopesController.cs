using AZM.Abyan.Identity.Application.Commands.Scope.Create;
using AZM.Abyan.Identity.Application.Commands.Scope.Delete;
using AZM.Abyan.Identity.Application.Commands.Scope.Update;
using AZM.Abyan.Identity.Application.DTOs.Scopes;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class ScopesController(IMediator mediator, IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

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
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
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
                return NotFound(new { message = _localizer["ScopeNotFound"] });

            return Ok(scope);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
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
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("{scopeId}")]
    public async Task<ActionResult> UpdateScope(string realm, string clientId, string scopeId, [FromBody] UpdateScopeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(scopeId, out var scopeIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidScopeId"] });
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
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("{scopeId}")]
    public async Task<ActionResult> DeleteScope(string realm, string clientId, string scopeId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(scopeId, out var scopeIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidScopeId"] });
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
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }
}
