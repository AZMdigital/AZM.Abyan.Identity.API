using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController(ISyncOrchestratorService syncOrchestratorService) : ControllerBase
{
    private readonly ISyncOrchestratorService _syncOrchestratorService = syncOrchestratorService;

    /// <summary>
    /// Sync all entities from Keycloak to local database
    /// </summary>
    /// <returns>Sync result with statistics and errors</returns>
    [HttpPost]
    public async Task<ActionResult<SyncResult>> SyncAll(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _syncOrchestratorService.SyncAllAsync(cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new SyncResult
            {
                Success = false,
                Errors = new List<string> { ex.Message }
            });
        }
    }
}

