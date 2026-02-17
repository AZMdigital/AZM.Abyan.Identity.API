using AZM.Abyan.Identity.Application.Commands.Client.Create;
using AZM.Abyan.Identity.Application.Commands.Client.Delete;
using AZM.Abyan.Identity.Application.Commands.Client.Update;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Queries.Client.GetClientById;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private IMediator? _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;
    public ClientsController(IClientService clientService, IMediator Mediator, IStringLocalizer<SharedResource> localizer)
    {
        _clientService = clientService;
        _mediator = Mediator;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientResponse>>> GetClients(string realm, CancellationToken cancellationToken)
    {
        try
        {
            var clients = await _clientService.GetClientsAsync(realm, cancellationToken);
            List<ClientResponse> Response = new List<ClientResponse>();
            if (clients.Count > 0)
            {
                foreach (var client in clients)
                {
                    ClientResponse clientResponse = new ClientResponse();
                    clientResponse.Name = client.Name;
                    clientResponse.Description = client.Description;
                    clientResponse.ClientId = client.ClientId;
                    clientResponse.RedirectUris = client.RedirectUris;
                    var getId = await _mediator.Send(new GetClientByKeycloakIdQuery(client.Id));
                    clientResponse.Id = getId.Data;
                    Response.Add(clientResponse);
                }
            }
            return Ok(Response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpGet("{clientName}")]
    public async Task<ActionResult<ClientResponse>> GetClientById(string realm, string clientName, CancellationToken cancellationToken)
    {
        try
        {
            var client = await _clientService.GetClientByIdAsync(realm, clientName, cancellationToken);
            ClientResponse clientResponse = new ClientResponse();
            clientResponse.Name = client.Name;
            clientResponse.Description = client.Description;
            clientResponse.ClientId = client.ClientId;
            clientResponse.RedirectUris = client.RedirectUris;
            var getId = await _mediator.Send(new GetClientByKeycloakIdQuery(client.Id));
            clientResponse.Id = getId.Data;

            if (client == null || clientResponse == null)
                return NotFound(new { message = _localizer["ClientNotFound"] });

            return Ok(clientResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }
    [HttpPost]
    public async Task<ActionResult> CreateClient(string realm, [FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Command handler will create client in Keycloak and save to database
            // Realm name comes from route parameter, RealmId will be resolved by the handler
            CreateClientCommand command = new CreateClientCommand();
            command.Name = request.Name;
            command.Description = request.Description;
            command.RedirectUris = request.RedirectUris;
            command.RealmName = realm; // Use realm name from route parameter
            
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateClient(string realm, Guid id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _clientService.UpdateClientAsync(realm, id.ToString(), request, cancellationToken);
            request.ClientId = id.ToString();
            var result = await _mediator.Send(new UpdateClientCommand(request));
            return Ok(new { message = _localizer["ClientUpdateSuccessfully"] });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteClient(string realm, string id, CancellationToken cancellationToken)
    {
        try
        {
            await _clientService.DeleteClientAsync(realm, id, cancellationToken);
            var resultDB = await _mediator.Send(new DeleteClientCommand(Guid.Parse(id)));
            return StatusCode(resultDB.StatusCode, resultDB);

        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    //[HttpPost("{id}/roles")]
    //public async Task<ActionResult> CreateClientRole(string realm, string id, [FromBody] CreateClientRoleRequest request, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        await _clientService.CreateClientRoleAsync(realm, id, request, cancellationToken);
    //        return Ok(new { message = $"Role '{request.Name}' created for client '{id}' in realm '{realm}' successfully" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

    //[HttpDelete("{id}/roles/{roleName}")]
    //public async Task<ActionResult> DeleteClientRole(string realm, string id, string roleName, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        await _clientService.DeleteClientRoleAsync(realm, id, roleName, cancellationToken);
    //        return Ok(new { message = $"Role '{roleName}' deleted from client '{id}' in realm '{realm}' successfully" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}
}

