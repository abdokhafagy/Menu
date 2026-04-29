using Menu.UI.State;

namespace Menu.UI.Services;

/// <summary>
/// Centralized permission and role check service for use in code-behind.
/// Delegates to UserContextService (populated from JWT by CustomAuthStateProvider).
/// Prefer this over raw UserContextService calls in components for cleaner semantics.
/// </summary>
public sealed class AuthorizationService
{
    private readonly UserContextService _userContext;

    public AuthorizationService(UserContextService userContext)
    {
        _userContext = userContext;
    }

    /// <summary>Returns true if the current user has the specified permission.</summary>
    public bool HasPermission(string permission) =>
        _userContext.HasPermission(permission);

    /// <summary>Returns true if the current user has ANY of the specified permissions.</summary>
    public bool HasAnyPermission(params string[] permissions) =>
        _userContext.HasAnyPermission(permissions);

    /// <summary>Returns true if the current user has ALL of the specified permissions.</summary>
    public bool HasAllPermissions(params string[] permissions) =>
        _userContext.HasAllPermissions(permissions);

    /// <summary>Returns true if the current user has the specified role (case-insensitive).</summary>
    public bool HasRole(string role) =>
        _userContext.HasRole(role);

    /// <summary>Returns true if the current user has ANY of the specified roles.</summary>
    public bool HasAnyRole(params string[] roles) =>
        _userContext.HasAnyRole(roles);

    /// <summary>Returns true if the current user is authenticated.</summary>
    public bool IsAuthenticated() =>
        _userContext.IsAuthenticated();

    /// <summary>Returns the current user's restaurant (tenant) ID from the JWT.</summary>
    public Guid? GetRestaurantId() =>
        _userContext.GetRestaurantId();

    /// <summary>Returns the current user's ID from the JWT.</summary>
    public Guid? GetUserId() =>
        _userContext.GetUserId();
}
