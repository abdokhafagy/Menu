using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Menu.UI.State;

using Microsoft.AspNetCore.Components.Authorization;

namespace Menu.UI.Auth;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly TokenService _tokenService;
    private readonly UserContextService _userContextService;

    public CustomAuthStateProvider(
        TokenService tokenService,
        UserContextService userContextService)
    {
        _tokenService = tokenService;
        _userContextService = userContextService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await _tokenService.GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            _userContextService.SetUser(null);
            return Anonymous;
        }

        var principal = BuildPrincipalFromToken(accessToken);
        if (principal is null)
        {
            _userContextService.SetUser(null);
            return Anonymous;
        }

        // Keep the identity if a refresh token exists so the HTTP pipeline
        // can rotate tokens transparently on the first authenticated request.
        if (IsTokenExpired(accessToken))
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _userContextService.SetUser(null);
                return Anonymous;
            }
        }

        _userContextService.SetUser(principal);
        return new AuthenticationState(principal);
    }

    public void NotifyUserAuthentication(string token)
    {
        var principal = BuildPrincipalFromToken(token) ?? Anonymous.User;
        _userContextService.SetUser(principal);
        _userContextService.ClearCache();
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(principal)));
    }

    public void NotifyUserLogout()
    {
        _userContextService.SetUser(null);
        _userContextService.ClearCache();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
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
                    continue;

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

                if (claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                    claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, claimValue));

                    var normalized = claimValue.ToLowerInvariant();
                    if (!claimValue.Equals(normalized, StringComparison.Ordinal))
                    {
                        // Add a lowercase variant to avoid casing issues in UI role checks.
                        claims.Add(new Claim(ClaimTypes.Role, normalized));
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
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}
