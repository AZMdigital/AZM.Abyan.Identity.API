using AZM.Abyan.Identity.Application.Commands.Client.Create;
using AZM.Abyan.Identity.Application.Commands.Client.Delete;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private IMediator? _mediator;
    public ClientsController(IClientService clientService, IMediator Mediator)
    {
        _clientService = clientService;
        _mediator = Mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientResponse>>> GetClients(string realm, CancellationToken cancellationToken)
    {
        try
        {
            var clients = await _clientService.GetClientsAsync(realm, cancellationToken);
            return Ok(clients);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientResponse>> GetClientById(string realm, string id, CancellationToken cancellationToken)
    {
        try
        {
            var client = await _clientService.GetClientByIdAsync(realm, id, cancellationToken);
            if (client == null)
                return NotFound(new { message = $"Client with id {id} not found in realm {realm}" });

            return Ok(client);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPost]
    public async Task<ActionResult> CreateClient(string realm, [FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        try
        {
           
            var result= await _clientService.CreateClientAsync(realm, request, cancellationToken);
            CreateClientCommand command = new CreateClientCommand();
            command.Name = request.Name;
            command.Description = request.Description;
            command.RealmId = request.RealmId;
            command.KeycloakClientId = result;
            var resultDB = await _mediator.Send(command);
            return StatusCode(resultDB.StatusCode, resultDB);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateClient(string realm, string id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _clientService.UpdateClientAsync(realm, id, request, cancellationToken);
            return Ok(new { message = $"Client '{id}' updated successfully in realm '{realm}'" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
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
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/roles")]
    public async Task<ActionResult> CreateClientRole(string realm, string id, [FromBody] CreateClientRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _clientService.CreateClientRoleAsync(realm, id, request, cancellationToken);
            return Ok(new { message = $"Role '{request.Name}' created for client '{id}' in realm '{realm}' successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/roles/{roleName}")]
    public async Task<ActionResult> DeleteClientRole(string realm, string id, string roleName, CancellationToken cancellationToken)
    {
        try
        {
            await _clientService.DeleteClientRoleAsync(realm, id, roleName, cancellationToken);
            return Ok(new { message = $"Role '{roleName}' deleted from client '{id}' in realm '{realm}' successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

