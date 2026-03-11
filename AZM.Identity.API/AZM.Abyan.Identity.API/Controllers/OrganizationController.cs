using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Organization.Create;
using AZM.Abyan.Identity.Application.Commands.Organization.Delete;
using AZM.Abyan.Identity.Application.Commands.Organization.Update;
using AZM.Abyan.Identity.Application.Commands.Organization.AddMember;
using AZM.Abyan.Identity.Application.Commands.Organization.RemoveMember;
using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.Queries.Organization.GetAll;
using AZM.Abyan.Identity.Application.Queries.Organization.GetById;
using AZM.Abyan.Identity.Application.Queries.Organization.GetMembers;
using Microsoft.AspNetCore.Mvc;
using AZM.Abyan.Identity.Application.DTOs.Groups;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class OrganizationController : BaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetAll(
        string realm,
        [FromQuery] string? search,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllOrganizationsQuery
        {
            RealmName = realm,
            Search = search
        };

        var result = await Mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetById(
        string realm,
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrganizationByIdQuery(realm, id);
        var result = await Mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(
        string realm,
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateOrganizationCommand
        {
            Name = request.Name,
            Description = request.Description,
            Alias = request.Alias,
            Domains = request.Domains!,
            Enabled = request.Enabled,
            RealmName = realm
        };

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        var getQuery = new GetOrganizationByIdQuery(realm, result.Data.ToString());
        var getResult = await Mediator.Send(getQuery, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { realm, id = result.Data },
            getResult.Data ?? new OrganizationResponse { Id = result.Data.ToString(), Name = request.Name });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(
        string realm,
        string id,
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateOrganizationCommand(Guid.Parse(id), request, realm);
        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(
        string realm,
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteOrganizationCommand(Guid.Parse(id), realm);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpGet("{id}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetMembers(
        string realm,
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMembersQuery(realm, id);
        var result = await Mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddMember(
        string realm,
        string id,
        [FromBody] AddMemberToOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddMemberToOrganizationCommand(realm, id, request.UserId);
        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpDelete("{id}/members/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveMember(
        string realm,
        string id,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveMemberFromOrganizationCommand(realm, id, memberId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }
}
