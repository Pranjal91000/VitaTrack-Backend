using Microsoft.AspNetCore.Mvc;
using VitaTrack.Core.Auth;
using VitaTrack.Core.Interfaces;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken token)
    {
        try
        {
            return Ok(await _identityService.RegisterAsync(request, token));
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken token)
    {
        try
        {
            return Ok(await _identityService.LoginAsync(request, token));
        }
        catch (Exception ex) when (ex.Message.Contains("Invalid"))
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken token)
    {
        try
        {
            return Ok(await _identityService.RefreshTokenAsync(request, token));
        }
        catch (Exception ex) when (ex.Message.Contains("Invalid") || ex.Message.Contains("refresh"))
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
