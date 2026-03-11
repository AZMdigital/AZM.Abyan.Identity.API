using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Resource.Create;
using AZM.Abyan.Identity.Application.Commands.Resource.Delete;
using AZM.Abyan.Identity.Application.Commands.Resource.Update;
using AZM.Abyan.Identity.Application.DTOs.Resources;
using AZM.Abyan.Identity.Application.Queries.Resource.GetResourceById;
using AZM.Abyan.Identity.Application.Queries.Resource.GetResources;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class ResourcesController : BaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetResources(string realm, string clientId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetResourcesQuery(realm, clientId), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{resourceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetResourceById(string realm, string clientId, Guid resourceId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetResourceByIdQuery(realm, clientId, resourceId), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateResource(string realm, Guid clientId, [FromBody] CreateResourceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateResourceCommand
        {
            CreateResourceRequest = request,
            RealmName = realm,
            ClientId = clientId,
            KeycloakClientId = clientId.ToString()
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{resourceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateResource(string realm, string clientId, Guid resourceId, [FromBody] UpdateResourceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateResourceCommand
        {
            ResourceId = resourceId,
            UpdateResourceRequest = request,
            RealmName = realm,
            KeycloakClientId = clientId
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpDelete("{resourceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteResource(string realm, string clientId, Guid resourceId, CancellationToken cancellationToken)
    {
        var command = new DeleteResourceCommand
        {
            ResourceId = resourceId,
            RealmName = realm,
            KeycloakClientId = clientId
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }
}
