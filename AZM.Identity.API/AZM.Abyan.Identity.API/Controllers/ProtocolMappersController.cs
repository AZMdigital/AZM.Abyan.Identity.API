using AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/client-scopes/{clientScopeId}/protocol-mappers")]
public class ProtocolMappersController : ControllerBase
{
    private readonly IKeycloakService _keycloakService;

    public ProtocolMappersController(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    [HttpPost]
    public async Task<ActionResult<ProtocolMapperResponse>> CreateProtocolMapper(
        string realm,
        string clientScopeId,
        [FromBody] CreateProtocolMapperRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var mapper = await _keycloakService.CreateProtocolMapperAsync(realm, clientScopeId, request, adminToken, cancellationToken);
            return CreatedAtAction(nameof(CreateProtocolMapper), new { realm, clientScopeId, mapperId = mapper.Id }, mapper);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{mapperId}/disable")]
    public async Task<ActionResult> DisableProtocolMapper(
        string realm,
        string clientScopeId,
        string mapperId,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            await _keycloakService.DisableProtocolMapperAsync(realm, clientScopeId, mapperId, adminToken, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
