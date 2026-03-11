using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.License.ActivateLicense;
using AZM.Abyan.Identity.Application.Commands.License.Create;
using AZM.Abyan.Identity.Application.Commands.License.Delete;
using AZM.Abyan.Identity.Application.Commands.License.RefreshToken;
using AZM.Abyan.Identity.Application.Commands.License.Update;
using AZM.Abyan.Identity.Application.DTOs;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Licenses;
using AZM.Abyan.Identity.Application.Queries.License.GetAllLicenses;
using AZM.Abyan.Identity.Application.Queries.License.GetLicenseById;
using AZM.Abyan.Identity.Application.Queries.License.GetSignedLicense;
using AZM.Abyan.Identity.Application.Queries.License.ValidateLicense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicensesController : BaseController
{

    /// <summary>
    /// Get all licenses
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<LicenseResponse>>> GetAllLicenses(CancellationToken cancellationToken)
    {
        try
        {
            var result = await Mediator.Send(new GetAllLicensesQuery(), cancellationToken);
            var licenses = result.IsSuccess ? result.Data ?? new List<LicenseResponse>() : new List<LicenseResponse>();
            return Ok(licenses);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = Localizer["OperationFailed"] ?? ex.Message });
        }
    }

    ///// <summary>
    ///// Get license by ID
    ///// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LicenseResponse>> GetLicenseById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await Mediator.Send(new GetLicenseByIdQuery(id), cancellationToken);
            if (!result.IsSuccess || result.Data == null)
                return NotFound(new { message = Localizer["LicenseNotFound"] });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = Localizer["OperationFailed"] ?? ex.Message });
        }
    }

    /// <summary>
    /// Create a new license
    /// </summary>
    /// <summary>
    /// Create a new license
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LicenseFileDto>> CreateLicense([FromBody] CreateLicenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateLicenseCommand
            {
                TenantId = request.TenantId,
                ClientNames = request.ClientNames ?? [],
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                PackageName = request.PackageName,
                Domain = request.Domain,
                ServerIps = request.ServerIps
            };

            var result = await Mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = Localizer["OperationFailed"] ?? ex.Message });
        }
    }
    /// Update an existing license
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateLicense(Guid id, [FromBody] UpdateLicenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // request.LicenseId = id;
            var command = new UpdateLicenseCommand
            {
                LicenseId = id,
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                IsActive = request.IsActive,
                Domain = request.Domain,
                ServerIps = request.ServerIps
            };

            var result = await Mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = Localizer["OperationFailed"] ?? ex.Message });
        }
    }

    /// <summary>
    /// Delete a license
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteLicense(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await Mediator.Send(new DeleteLicenseCommand(id), cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = Localizer["OperationFailed"] ?? ex.Message });
        }
    }

    /// <summary>Activate a license file and receive a short-lived RS256 JWT with refresh token.</summary>
    [HttpPost("activate")]
    [AllowAnonymous]
    // [EnableRateLimiting("activation")] // Needs to be configured in DI if uncommented
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ActivateLicenseResponse>> Activate(
        [FromBody] ActivateLicenseRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ActivateLicenseCommand(request.LicenseFile), ct);
        return Ok(result);
    }

    /// <summary>Refresh access token using refresh token (when access token expires).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshAccessTokenResponse>> RefreshAccessToken(
        [FromBody] RefreshAccessTokenRequest request, CancellationToken ct)
    {
        try
        {
            var result = await Mediator.Send(
                new RefreshAccessTokenCommand(request.RefreshToken), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = Localizer["OperationFailed"] ?? ex.Message });
        }
    }

    /// <summary>Query license validity (used for revocation checks).</summary>
    [HttpGet("validate/{licenseId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ValidateLicenseResponse>> Validate(
        Guid licenseId, CancellationToken ct)
    {
        // Extract current domain and IP for validation
        var currentDomain = Request.Host.Host;
        var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        var result = await Mediator.Send(new ValidateLicenseQuery(licenseId, currentDomain, currentIp), ct);
        if (!result.IsValid) return Forbid();
        return Ok(result);
    }

    /// <summary>Get the signed license file JSON for a specific license record.</summary>
    [HttpGet("{id:guid}/signed")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LicenseFileDto>> GetSignedLicense(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await Mediator.Send(new GetSignedLicenseQuery(id), ct);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = Localizer["OperationFailed"] ?? ex.Message });
        }
    }
}
