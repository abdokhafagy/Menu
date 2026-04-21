using System.Security.Claims;

using Menu.Application.DTOs.Auth;

namespace Menu.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, string? ipAddress, CancellationToken ct = default);
    Task LogoutAsync(ClaimsPrincipal principal, CancellationToken ct = default);
    Task LogoutAllAsync(ClaimsPrincipal principal, CancellationToken ct = default);
    Task<IReadOnlyList<SessionDto>> GetSessionsAsync(ClaimsPrincipal principal, CancellationToken ct = default);
    Task RevokeSessionAsync(ClaimsPrincipal principal, Guid sessionId, CancellationToken ct = default);
}
