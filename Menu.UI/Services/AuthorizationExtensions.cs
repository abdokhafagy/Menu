using System.Security.Claims;

namespace Menu.UI.Services;

/// <summary>
/// Helper methods for authorization checks in Blazor components.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Checks if user is a SuperAdmin.
    /// </summary>
    public static bool IsSuperAdmin(this ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        return user.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Any(c => c.Value?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Checks if user has a specific permission.
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal? user, string permission)
    {
        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(permission))
            return false;

        if (user.IsSuperAdmin())
            return true;

        return user.FindAll("permissions")
            .Any(c => c.Value?.Equals(permission, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Checks if user has any of the specified permissions.
    /// </summary>
    public static bool HasAnyPermission(this ClaimsPrincipal? user, params string[] permissions)
    {
        if (user?.Identity?.IsAuthenticated != true || permissions == null || permissions.Length == 0)
            return false;

        if (user.IsSuperAdmin())
            return true;

        var userPermissions = user.FindAll("permissions")
            .Select(c => c.Value)
            .ToList();

        return permissions.Any(p =>
            userPermissions.Any(up => up?.Equals(p, StringComparison.OrdinalIgnoreCase) == true));
    }

    /// <summary>
    /// Checks if user has all of the specified permissions.
    /// </summary>
    public static bool HasAllPermissions(this ClaimsPrincipal? user, params string[] permissions)
    {
        if (user?.Identity?.IsAuthenticated != true || permissions == null || permissions.Length == 0)
            return true;

        if (user.IsSuperAdmin())
            return true;

        var userPermissions = user.FindAll("permissions")
            .Select(c => c.Value)
            .ToList();

        return permissions.All(p =>
            userPermissions.Any(up => up?.Equals(p, StringComparison.OrdinalIgnoreCase) == true));
    }

    /// <summary>
    /// Gets the user's restaurant ID (tenant context) from JWT.
    /// </summary>
    public static Guid? GetRestaurantId(this ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var restaurantIdClaim = user.FindFirst("restaurantId");
        if (Guid.TryParse(restaurantIdClaim?.Value, out var restaurantId))
            return restaurantId;

        return null;
    }

    /// <summary>
    /// Gets the user's ID from JWT.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = user.FindFirst("userId") ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim?.Value, out var userId))
            return userId;

        return null;
    }

    /// <summary>
    /// Gets the user's email from JWT.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal? user)
    {
        return user?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets all roles for the user.
    /// </summary>
    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return Array.Empty<string>();

        return user.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Gets all permissions for the user.
    /// </summary>
    public static IReadOnlyList<string> GetPermissions(this ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return Array.Empty<string>();

        return user.FindAll("permissions")
            .Select(c => c.Value)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
