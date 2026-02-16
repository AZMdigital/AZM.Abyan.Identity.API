using AZM.Abyan.Identity.Application.Commands.Resource.Create;
using AZM.Abyan.Identity.Application.Commands.Resource.Delete;
using AZM.Abyan.Identity.Application.Commands.Resource.Update;
using AZM.Abyan.Identity.Application.DTOs.Resources;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IKeycloakService _keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ResourcesController(IMediator mediator, IKeycloakService keycloakService, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _keycloakService = keycloakService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult> GetResources(string realm, string clientId, CancellationToken cancellationToken)
    {
        try
        {
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var resources = await _keycloakService.GetAllResourcesAsync(realm, clientId, adminToken, cancellationToken);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpGet("{resourceId}")]
    public async Task<ActionResult> GetResourceById(string realm, string clientId, string resourceId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(resourceId, out var resourceIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidResourceId"] });
            }

            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);
            var resource = await _keycloakService.GetResourceByIdAsync(realm, clientId, resourceIdGuid, adminToken, cancellationToken);
            
            if (resource == null)
                return NotFound(new { message = _localizer["ResourceNotFound"] });

            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateResource(string realm, string clientId, [FromBody] CreateResourceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(clientId, out var clientIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidClientId"] });
            }

            var command = new CreateResourceCommand
            {
                CreateResourceRequest = request,
                RealmName = realm,
                ClientId = clientIdGuid,
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

    [HttpPut("{resourceId}")]
    public async Task<ActionResult> UpdateResource(string realm, string clientId, string resourceId, [FromBody] UpdateResourceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(resourceId, out var resourceIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidResourceId"] });
            }

            var command = new UpdateResourceCommand
            {
                ResourceId = resourceIdGuid,
                UpdateResourceRequest = request,
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

    [HttpDelete("{resourceId}")]
    public async Task<ActionResult> DeleteResource(string realm, string clientId, string resourceId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(resourceId, out var resourceIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidResourceId"] });
            }

            var command = new DeleteResourceCommand
            {
                ResourceId = resourceIdGuid,
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
}
