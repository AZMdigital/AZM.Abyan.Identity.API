using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
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
            await _roleService.CreateClientRoleAsync(realm, clientId, request, cancellationToken);
            return Ok(new { message = $"Role '{request.Name}' created successfully for client '{clientId}' in realm '{realm}'" });
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

