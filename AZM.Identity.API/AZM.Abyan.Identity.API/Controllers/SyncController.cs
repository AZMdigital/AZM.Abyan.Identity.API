using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Sync.SyncAll;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : BaseController
{
    /// <summary>
    /// Sync all entities from Keycloak to local database
    /// </summary>
    /// <returns>Sync result with statistics and errors</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SyncResult>> SyncAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SyncAllCommand(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
