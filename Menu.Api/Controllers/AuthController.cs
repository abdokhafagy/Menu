using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Api.Filters;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Auth;
using Menu.Application.Interfaces;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [Authorize]
    [RequirePermission("users.create")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> Logout(CancellationToken ct)
    {
        await _authService.LogoutAsync(User, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Logged out."));
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> LogoutAll(CancellationToken ct)
    {
        await _authService.LogoutAllAsync(User, ct);
        return Ok(ApiResponse<string>.SuccessResponse("All sessions revoked."));
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SessionDto>>>> Sessions(CancellationToken ct)
    {
        var sessions = await _authService.GetSessionsAsync(User, ct);
        return Ok(ApiResponse<IReadOnlyList<SessionDto>>.SuccessResponse(sessions));
    }

    [HttpPost("sessions/{sessionId:guid}/revoke")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> RevokeSession(Guid sessionId, CancellationToken ct)
    {
        await _authService.RevokeSessionAsync(User, sessionId, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Session revoked."));
    }
}
