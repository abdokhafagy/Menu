using System.Security.Claims;

using Menu.Domain.Entities;

namespace Menu.Application.Interfaces;

public interface ITokenService
{
    Task<(string accessToken, string jti, DateTime expiresAt)> GenerateAccessTokenAsync(
        User user,
        Guid sessionId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        CancellationToken ct = default);

    string GenerateRefreshToken();
    string HashToken(string token);
    ClaimsPrincipal? ValidateExpiredToken(string token);
}
