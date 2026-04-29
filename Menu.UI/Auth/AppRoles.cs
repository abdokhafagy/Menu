namespace Menu.UI.Auth;

/// <summary>
/// Frontend mirror of the backend RoleNames constants.
/// These strings must exactly match Menu.Domain.Authorization.RoleNames.
/// Use these constants instead of inline string literals in AuthorizeView / PermissionView.
/// </summary>
public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin      = "Admin";
    public const string Manager    = "Manager";
    public const string User       = "User";
}
