using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Client.Create;
using AZM.Abyan.Identity.Application.Commands.Client.Delete;
using AZM.Abyan.Identity.Application.Commands.Client.Update;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.Queries.Client.GetClientByName;
using AZM.Abyan.Identity.Application.Queries.Client.GetClients;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class ClientsController : BaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetClients(string realm, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetClientsQuery(realm), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{clientName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetClientById(string realm, string clientName, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetClientByNameQuery(realm, clientName), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateClient(string realm, [FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateClientCommand
        {
            Name = request.Name,
            Description = request.Description,
            RedirectUris = request.RedirectUris,
            RealmName = realm
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateClient(string realm, Guid id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        request.ClientId = id.ToString();
        var result = await Mediator.Send(new UpdateClientCommand(realm, request), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteClient(string realm, Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteClientCommand(realm, id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
