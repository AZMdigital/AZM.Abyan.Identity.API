using LocalPolicy.DTOs;
using LocalPolicy.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalPolicy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IKeycloakAuthService _keycloakAuthService;

    public AuthController(IKeycloakAuthService keycloakAuthService)
    {
        _keycloakAuthService = keycloakAuthService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _keycloakAuthService.LoginAsync(request.Username, request.Password, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

