using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace Menu.UI.Auth;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private readonly TokenService _tokenService;

    public CustomAuthStateProvider(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await _tokenService.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new AuthenticationState(Anonymous);
        }

        var principal = BuildPrincipalFromToken(accessToken);
        if (principal is null)
        {
            return new AuthenticationState(Anonymous);
        }

        // Keep the identity if a refresh token exists so HTTP pipeline can rotate tokens.
        if (IsTokenExpired(accessToken))
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new AuthenticationState(Anonymous);
            }
        }

        return new AuthenticationState(principal);
    }

    public void NotifyUserAuthentication(string token)
    {
        var principal = BuildPrincipalFromToken(token) ?? Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static ClaimsPrincipal? BuildPrincipalFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = new List<Claim>();
            foreach (var claim in jwt.Claims)
            {
                var claimValue = claim.Value?.Trim();
                if (string.IsNullOrWhiteSpace(claimValue))
                {
                    continue;
                }

                if (claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, claimValue));
                    continue;
                }

                if (claim.Type.Equals("email", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Email, claimValue));
                    continue;
                }

                if (claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase) || claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase))
                {
                    var roleValue = claimValue;
                    if (!string.IsNullOrWhiteSpace(roleValue))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleValue));

                        var normalizedRole = roleValue.ToLowerInvariant();
                        if (!roleValue.Equals(normalizedRole, StringComparison.Ordinal))
                        {
                            // Add a lowercase role variant to avoid casing issues in UI role checks.
                            claims.Add(new Claim(ClaimTypes.Role, normalizedRole));
                        }
                    }
                    continue;
                }

                claims.Add(new Claim(claim.Type, claimValue));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        }
        catch
        {
            return null;
        }
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}
