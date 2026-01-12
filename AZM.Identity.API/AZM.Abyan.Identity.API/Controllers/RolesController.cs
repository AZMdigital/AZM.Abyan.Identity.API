using AZM.Abyan.Identity.Application.Commands.Role.Create;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IMediator _mediator;

    public RolesController(IRoleService roleService, IMediator mediator)
    {
        _roleService = roleService;
        _mediator = mediator;
    }

    [HttpGet("clients/{clientId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClientRoleResponse>>> GetClientRoles(string realm, string clientId, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetClientRolesAsync(realm, clientId, cancellationToken);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("clients/{clientId}")]
    public async Task<ActionResult> CreateClientRole(string realm, string clientId, [FromBody] CreateClientRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Command handler will create role in Keycloak and save to database
            // clientId parameter is the Keycloak client ID (string), we need to parse it to Guid for local ClientId
            if (!Guid.TryParse(clientId, out var clientIdGuid))
            {
                return BadRequest(new { message = "Invalid client ID format" });
            }

            CreateRoleCommand command = new CreateRoleCommand();
            command.Name = request.Name;
            command.Description = request.Description;
            command.Realm = realm;
            command.KeycloakClientId = clientId; // Keycloak client ID (string)
            command.ClientId = clientIdGuid; // Local client ID (Guid, same as Keycloak ID now)
            
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("clients/{clientId}/{roleName}")]
    public async Task<ActionResult> DeleteClientRole(string realm, string clientId, string roleName, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.DeleteClientRoleAsync(realm, clientId, roleName, cancellationToken);
            return Ok(new { message = $"Role '{roleName}' deleted successfully from client '{clientId}' in realm '{realm}'" });
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

    [HttpPost("assign")]
    public async Task<ActionResult> AssignClientRoleToUser(string realm, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.AssignClientRoleToUserAsync(realm, request, cancellationToken);
            return Ok(new { message = $"Role {request.RoleName} assigned to user {request.UserId} successfully in realm '{realm}'" });
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

    [HttpPost("unassign")]
    public async Task<ActionResult> UnassignClientRoleFromUser(string realm, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.RemoveClientRoleFromUserAsync(realm, request, cancellationToken);
            return Ok(new { message = $"Role {request.RoleName} removed from user {request.UserId} successfully in realm '{realm}'" });
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

