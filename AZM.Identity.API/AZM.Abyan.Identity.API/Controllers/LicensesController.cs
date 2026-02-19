using AZM.Abyan.Identity.Application.Commands.License.Create;
using AZM.Abyan.Identity.Application.Commands.License.Delete;
using AZM.Abyan.Identity.Application.Commands.License.Update;
using AZM.Abyan.Identity.Application.DTOs.Licenses;
using AZM.Abyan.Identity.Application.Queries.License.GetAllLicenses;
using AZM.Abyan.Identity.Application.Queries.License.GetLicenseById;
using AZM.Abyan.Identity.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicensesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LicensesController(
        IMediator mediator,
        IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    /// <summary>
    /// Get all licenses
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<LicenseResponse>>> GetAllLicenses(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetAllLicensesQuery(), cancellationToken);
            var licenses = result.IsSuccess ? result.Data ?? new List<LicenseResponse>() : new List<LicenseResponse>();
            return Ok(licenses);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    ///// <summary>
    ///// Get license by ID
    ///// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LicenseResponse>> GetLicenseById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetLicenseByIdQuery(id), cancellationToken);
            if (!result.IsSuccess || result.Data == null)
                return NotFound(new { message = _localizer["LicenseNotFound"] ?? "License not found" });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    /// <summary>
    /// Create a new license
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateLicense([FromBody] CreateLicenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateLicenseCommand
            {
                TenantId = request.TenantId,
                ClientId = request.ClientId,
                LicenseKeyHash = request.LicenseKeyHash,
                PublicKey = request.PublicKey,
                PrivateKeyEncrypted = request.PrivateKeyEncrypted,
                IssuedAt = request.IssuedAt,
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                PackageName = request.PackageName,
                Domain = request.Domain,
                ServerIps = request.ServerIps
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    /// <summary>
    /// Update an existing license
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateLicense(Guid id, [FromBody] UpdateLicenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.LicenseId = id;
            var command = new UpdateLicenseCommand
            {
                LicenseId = request.LicenseId,
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                IsRevoked = request.IsRevoked,
                Domain = request.Domain,
                ServerIps = request.ServerIps
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    /// <summary>
    /// Delete a license
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLicense(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new DeleteLicenseCommand(id), cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }
}
