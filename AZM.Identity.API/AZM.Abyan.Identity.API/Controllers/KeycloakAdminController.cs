using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

/// <summary>
/// Keycloak Admin Controller for Multi-Tenancy Management
/// Provides endpoints for super admin operations including realm and role management
/// </summary>
[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class KeycloakAdminController(IRealmAdminService realmAdminService) : ControllerBase
{
    private readonly IRealmAdminService _realmAdminService = realmAdminService;

    /// <summary>
    /// Get all realms (tenants) in Keycloak
    /// </summary>
    /// <returns>List of all realms</returns>
    [HttpGet("realms")]
    public async Task<ActionResult<List<RealmResponse>>> GetAllRealms(CancellationToken cancellationToken)
    {
        try
        {
            var realms = await _realmAdminService.GetAllRealmsAsync(cancellationToken);
            return Ok(realms);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific realm by name
    /// </summary>
    /// <param name="realmName">The realm name</param>
    /// <returns>Realm details</returns>
    [HttpGet("realms/{realmName}")]
    public async Task<ActionResult<RealmResponse>> GetRealmByName(string realmName, CancellationToken cancellationToken)
    {
        try
        {
            var realm = await _realmAdminService.GetRealmByNameAsync(realmName, cancellationToken);
            if (realm == null)
                return NotFound(new { message = $"Realm '{realmName}' not found" });

            return Ok(realm);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new realm (tenant)
    /// </summary>
    /// <param name="request">Realm creation request</param>
    /// <returns>Success message</returns>
    [HttpPost("realms")]
    public async Task<ActionResult> CreateRealm([FromBody] CreateRealmRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.CreateRealmAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetRealmByName),
                new { realmName = request.Realm },
                new { message = $"Realm '{request.Realm}' created successfully" }
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("realms/{realmName}")]
    public async Task<ActionResult> UpdateRealm(string realmName, [FromBody] UpdateRealmRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.UpdateRealmAsync(realmName, request, cancellationToken);
            return Ok(new { message = $"Realm '{realmName}' updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("realms/{realmName}/password-policy")]
    public async Task<ActionResult> UpdateRealmPasswordPolicy(string realmName, [FromBody] UpdateRealmPasswordPolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.UpdateRealmPasswordPolicyAsync(realmName, request, cancellationToken);
            return Ok(new { message = $"Password policy for realm '{realmName}' updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("realms/{realmName}")]
    public async Task<ActionResult> DeleteRealm(string realmName, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.DeleteRealmAsync(realmName, cancellationToken);
            return Ok(new { message = $"Realm '{realmName}' deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all roles in a specific realm
    /// </summary>
    /// <param name="realm">The realm name</param>
    /// <returns>List of realm roles</returns>
    [HttpGet("realms/{realm}/roles")]
    public async Task<ActionResult<List<RealmRoleResponse>>> GetRealmRoles(string realm, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _realmAdminService.GetRealmRolesAsync(realm, cancellationToken);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new role in a realm
    /// </summary>
    /// <param name="request">Role creation request</param>
    /// <returns>Success message</returns>
    [HttpPost("realms/roles")]
    public async Task<ActionResult> CreateRealmRole([FromBody] CreateRealmRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.CreateRealmRoleAsync(request, cancellationToken);
            return Ok(new { message = $"Role '{request.Name}' created successfully in realm '{request.Realm}'" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("realms/{realm}/roles/{roleName}")]
    public async Task<ActionResult> UpdateRealmRole(string realm, string roleName, [FromBody] UpdateRealmRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.UpdateRealmRoleAsync(realm, roleName, request, cancellationToken);
            return Ok(new { message = $"Role '{roleName}' updated successfully in realm '{realm}'" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("realms/{realm}/roles/{roleName}")]
    public async Task<ActionResult> DeleteRealmRole(string realm, string roleName, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.DeleteRealmRoleAsync(realm, roleName, cancellationToken);
            return Ok(new { message = $"Role '{roleName}' deleted successfully from realm '{realm}'" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Assign a realm role to a user
    /// </summary>
    /// <param name="request">Role assignment request</param>
    /// <returns>Success message</returns>
    [HttpPost("roles/assign")]
    public async Task<ActionResult> AssignRealmRoleToUser([FromBody] AssignRealmRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.AssignRealmRoleToUserAsync(request, cancellationToken);
            return Ok(new { message = $"Role '{request.RoleName}' assigned to user '{request.UserId}' in realm '{request.Realm}'" });
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

    /// <summary>
    /// Remove a realm role from a user
    /// </summary>
    /// <param name="request">Role removal request</param>
    /// <returns>Success message</returns>
    [HttpDelete("roles/remove")]
    public async Task<ActionResult> RemoveRealmRoleFromUser([FromBody] AssignRealmRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _realmAdminService.RemoveRealmRoleFromUserAsync(request, cancellationToken);
            return Ok(new { message = $"Role '{request.RoleName}' removed from user '{request.UserId}' in realm '{request.Realm}'" });
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
