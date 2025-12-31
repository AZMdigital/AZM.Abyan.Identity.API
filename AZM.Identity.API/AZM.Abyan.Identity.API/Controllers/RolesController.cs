using AZM.Identity.Application.DTOs.Roles;
using AZM.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("clients/{clientId}")]
    public async Task<ActionResult<List<ClientRoleResponse>>> GetClientRoles(string clientId, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetClientRolesAsync(clientId, cancellationToken);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("assign")]
    public async Task<ActionResult> AssignClientRoleToUser([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.AssignClientRoleToUserAsync(request, cancellationToken);
            return Ok(new { message = $"Role {request.RoleName} assigned to user {request.UserId} successfully" });
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

    [HttpPost("remove")]
    public async Task<ActionResult> RemoveClientRoleFromUser([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.RemoveClientRoleFromUserAsync(request, cancellationToken);
            return Ok(new { message = $"Role {request.RoleName} removed from user {request.UserId} successfully" });
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

