using AZM.Abyan.Identity.Application.Commands.Organization.Create;
using AZM.Abyan.Identity.Application.Commands.Organization.Delete;
using AZM.Abyan.Identity.Application.Commands.Organization.Update;
using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Queries.Organization.GetAll;
using AZM.Abyan.Identity.Application.Queries.Organization.GetById;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

/// <summary>
/// CRUD, members, and organization roles for Keycloak Organizations (Keycloak 25+ Organizations API).
/// Organization roles are scoped to the org; use the roles and member-role endpoints to add roles and assign users to them.
/// Handles both Keycloak API calls and database persistence simultaneously.
/// </summary>
[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly IKeycloakService _keycloakService;
    private readonly IOrganizationService _organizationService;
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public OrganizationController(
        IKeycloakService keycloakService,
        IOrganizationService organizationService,
        IMediator mediator,
        IStringLocalizer<SharedResource> localizer)
    {
        _keycloakService = keycloakService;
        _organizationService = organizationService;
        _mediator = mediator;
        _localizer = localizer;
    }

    // ---------- CRUD ----------

    [HttpGet]
    public async Task<ActionResult<List<OrganizationResponse>>> GetAll(
        string realm,
        [FromQuery] string? search,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAllOrganizationsQuery
            {
                RealmName = realm,
                Search = search
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrganizationResponse>> GetById(
        string realm,
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetOrganizationByIdQuery(realm, id);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(new { message = result.Message ?? _localizer["ResourceNotFound"] });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationResponse>> Create(
        string realm,
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreateOrganizationCommand
            {
                Name = request.Name,
                Description = request.Description,
                Alias = request.Alias,
                Domains = request.Domains,
                Enabled = request.Enabled,
                RealmName = realm
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            // Get the created organization from Keycloak
            var getQuery = new GetOrganizationByIdQuery(realm, result.Data.ToString());
            var getResult = await _mediator.Send(getQuery, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { realm, id = result.Data },
                getResult.Data ?? new OrganizationResponse { Id = result.Data.ToString(), Name = request.Name });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        string realm,
        string id,
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateOrganizationCommand(Guid.Parse(id), request, realm);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(
        string realm,
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteOrganizationCommand(Guid.Parse(id), realm);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    // ---------- Members (add/remove user from organization) ----------

    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<UserResponse>>> GetMembers(
        string realm,
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _organizationService.GetOrganizationMembersAsync(realm, id, cancellationToken);
            return Ok(members);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost("{id}/members")]
    public async Task<ActionResult> AddMember(
        string realm,
        string id,
        [FromBody] AddMemberToOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _organizationService.AddMemberToOrganizationAsync(realm, id, request.UserId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("{id}/members/{memberId}")]
    public async Task<ActionResult> RemoveMember(
        string realm,
        string id,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _organizationService.RemoveMemberFromOrganizationAsync(realm, id, memberId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }
    #region  Organization roles
    // ---------- Organization roles (add role to organization) ----------

    //[HttpGet("{id}/roles")]
    //public async Task<ActionResult<List<OrganizationRoleResponse>>> GetRoles(string realm, string id, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
    //        var roles = await _keycloakService.GetOrganizationRolesAsync(realm, id, adminToken, cancellationToken);
    //        return Ok(roles);
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
    //    }
    //}

    //[HttpPost("{id}/roles")]
    //public async Task<ActionResult> CreateRole(string realm, string id, [FromBody] CreateOrganizationRoleRequest request, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
    //        await _keycloakService.CreateOrganizationRoleAsync(realm, id, request, adminToken, cancellationToken);
    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
    //    }
    //}

    //[HttpDelete("{id}/roles/{roleName}")]
    //public async Task<ActionResult> DeleteRole(string realm, string id, string roleName, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
    //        await _keycloakService.DeleteOrganizationRoleAsync(realm, id, roleName, adminToken, cancellationToken);
    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
    //    }
    //}

    // ---------- Assign user to organization role / revoke role from member ----------

    //[HttpGet("{id}/members/{memberId}/roles")]
    //public async Task<ActionResult<List<OrganizationRoleResponse>>> GetMemberRoles(string realm, string id, string memberId, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
    //        var roles = await _keycloakService.GetOrganizationMemberRolesAsync(realm, id, memberId, adminToken, cancellationToken);
    //        return Ok(roles);
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
    //    }
    //}

    //[HttpPost("{id}/members/{memberId}/roles")]
    //public async Task<ActionResult> AssignRolesToMember(string realm, string id, string memberId, [FromBody] AssignOrganizationRolesRequest request, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
    //        await _keycloakService.AssignOrganizationRolesToMemberAsync(realm, id, memberId, request, adminToken, cancellationToken);
    //        return NoContent();
    //    }
    //    catch (KeyNotFoundException ex)
    //    {
    //        return NotFound(new { message = ex.Message });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
    //    }
    //}

    //[HttpDelete("{id}/members/{memberId}/roles/{roleName}")]
    //public async Task<ActionResult> RemoveRoleFromMember(string realm, string id, string memberId, string roleName, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
    //        await _keycloakService.RemoveOrganizationRoleFromMemberAsync(realm, id, memberId, roleName, adminToken, cancellationToken);
    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
    //    }
    //}
    #endregion
}
