using System.Security.Claims;

namespace Menu.UI.State;

/// <summary>
/// Provides cached user context extracted from JWT claims.
/// Decoupled from AuthenticationStateProvider to break the circular DI cycle.
/// CustomAuthStateProvider pushes the resolved principal via SetUser().
/// </summary>
public sealed class UserContextService
{
    private ClaimsPrincipal? _cachedUser;
    private IReadOnlyList<string>? _cachedRoles;
    private IReadOnlyList<string>? _cachedPermissions;
    private Guid? _cachedUserId;
    private Guid? _cachedRestaurantId;

    public event Action? OnContextChanged;

    /// <summary>
    /// Sets the current user principal. Called by CustomAuthStateProvider
    /// after resolving auth state. Pass null to represent an anonymous user.
    /// </summary>
    public void SetUser(ClaimsPrincipal? user)
    {
        var previous = _cachedUser;
        _cachedUser = user;
        // Invalidate derived caches whenever the principal changes.
        _cachedRoles = null;
        _cachedPermissions = null;
        _cachedUserId = null;
        _cachedRestaurantId = null;

        if (!ReferenceEquals(previous, user))
        {
            OnContextChanged?.Invoke();
        }
    }

    /// <summary>Gets the current authenticated user's principal (sync, cached).</summary>
    public ClaimsPrincipal? GetUser() => _cachedUser;

    /// <summary>Gets the current user's ID from JWT claim (sync, cached).</summary>
    public Guid? GetUserId()
    {
        if (_cachedUserId.HasValue)
            return _cachedUserId;

        if (_cachedUser?.Identity?.IsAuthenticated != true)
            return null;

        var claim = _cachedUser.FindFirst("userId")
                 ?? _cachedUser.FindFirst(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(claim?.Value, out var userId))
            _cachedUserId = userId;

        return _cachedUserId;
    }

    /// <summary>Gets the current user's restaurant ID / tenant context (sync, cached).</summary>
    public Guid? GetRestaurantId()
    {
        if (_cachedRestaurantId.HasValue)
            return _cachedRestaurantId;

        if (_cachedUser?.Identity?.IsAuthenticated != true)
            return null;

        var claim = _cachedUser.FindFirst("restaurantId");

        if (Guid.TryParse(claim?.Value, out var restaurantId))
            _cachedRestaurantId = restaurantId;

        return _cachedRestaurantId;
    }

    /// <summary>Gets all roles for the current user (sync, cached).</summary>
    public IReadOnlyList<string> GetRoles()
    {
        if (_cachedRoles != null)
            return _cachedRoles;

        if (_cachedUser?.Identity?.IsAuthenticated != true)
            return _cachedRoles = Array.Empty<string>();

        _cachedRoles = _cachedUser
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return _cachedRoles;
    }

    /// <summary>Gets all permissions for the current user (sync, cached).</summary>
    public IReadOnlyList<string> GetPermissions()
    {
        if (_cachedPermissions != null)
            return _cachedPermissions;

        if (_cachedUser?.Identity?.IsAuthenticated != true)
            return _cachedPermissions = Array.Empty<string>();

        _cachedPermissions = _cachedUser
            .FindAll("permissions")
            .Select(c => c.Value)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return _cachedPermissions;
    }

    /// <summary>Checks if the user has a specific role (case-insensitive).</summary>
    public bool HasRole(string role) =>
        !string.IsNullOrWhiteSpace(role) &&
        GetRoles().Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));

    /// <summary>Checks if the user has any of the specified roles (case-insensitive).</summary>
    public bool HasAnyRole(params string[] roles) =>
        roles is { Length: > 0 } &&
        GetRoles().Any(r => roles.Any(x => x.Equals(r, StringComparison.OrdinalIgnoreCase)));

    /// <summary>Checks if the user has all of the specified roles (case-insensitive).</summary>
    public bool HasAllRoles(params string[] roles) =>
        roles is null || roles.Length == 0 ||
        roles.All(r => GetRoles().Any(x => x.Equals(r, StringComparison.OrdinalIgnoreCase)));

    /// <summary>True if the current user is a SuperAdmin (case-insensitive).</summary>
    public bool IsSuperAdmin() =>
        GetRoles().Any(r => r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));

    /// <summary>Checks if the user has a specific permission (case-insensitive). SuperAdmin bypasses.</summary>
    public bool HasPermission(string permission) =>
        IsSuperAdmin() ||
        (!string.IsNullOrWhiteSpace(permission) &&
         GetPermissions().Any(p => p.Equals(permission, StringComparison.OrdinalIgnoreCase)));

    /// <summary>Checks if the user has any of the specified permissions (case-insensitive). SuperAdmin bypasses.</summary>
    public bool HasAnyPermission(params string[] permissions) =>
        IsSuperAdmin() ||
        (permissions is { Length: > 0 } &&
         GetPermissions().Any(p => permissions.Any(x => x.Equals(p, StringComparison.OrdinalIgnoreCase))));

    /// <summary>Checks if the user has all of the specified permissions (case-insensitive). SuperAdmin bypasses.</summary>
    public bool HasAllPermissions(params string[] permissions) =>
        IsSuperAdmin() ||
        permissions is null || permissions.Length == 0 ||
        permissions.All(p => GetPermissions().Any(x => x.Equals(p, StringComparison.OrdinalIgnoreCase)));

    /// <summary>Checks if the current user is authenticated.</summary>
    public bool IsAuthenticated() =>
        _cachedUser?.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Clears all caches. Call after login, logout, or token refresh.
    /// </summary>
    public void ClearCache()
    {
        _cachedUser = null;
        _cachedRoles = null;
        _cachedPermissions = null;
        _cachedUserId = null;
        _cachedRestaurantId = null;
        OnContextChanged?.Invoke();
    }
}
