using Microsoft.AspNetCore.Components.Routing;

namespace Menu.UI.Navigation;

/// <summary>
/// Single entry in the application navigation. Rendered by both the sidebar
/// (NavMenu) and the mobile bottom bar (BottomNav). Visibility is computed
/// from the current user's roles + permissions; never hard-coded in markup.
/// </summary>
public sealed record NavItem(
    string TitleKey,
    string Icon,
    string Route,
    string[] RequiredRoles,
    string[] RequiredPermissions,
    NavLinkMatch Match = NavLinkMatch.Prefix,
    NavSection Section = NavSection.Management,
    bool ShowOnMobile = false,
    int MobilePriority = 100,
    bool RequireAuthenticated = true)
{
    public static NavItem Public(string titleKey, string icon, string route, NavSection section = NavSection.Overview, bool showOnMobile = false, int mobilePriority = 100, NavLinkMatch match = NavLinkMatch.Prefix)
        => new(titleKey, icon, route, Array.Empty<string>(), Array.Empty<string>(), match, section, showOnMobile, mobilePriority, RequireAuthenticated: false);

    public static NavItem Authenticated(string titleKey, string icon, string route, NavSection section = NavSection.Overview, bool showOnMobile = false, int mobilePriority = 100, NavLinkMatch match = NavLinkMatch.Prefix)
        => new(titleKey, icon, route, Array.Empty<string>(), Array.Empty<string>(), match, section, showOnMobile, mobilePriority, RequireAuthenticated: true);

    public static NavItem ByPermission(string titleKey, string icon, string route, string permission, NavSection section = NavSection.Management, bool showOnMobile = false, int mobilePriority = 100, NavLinkMatch match = NavLinkMatch.Prefix)
        => new(titleKey, icon, route, Array.Empty<string>(), new[] { permission }, match, section, showOnMobile, mobilePriority, RequireAuthenticated: true);

    public static NavItem ByRole(string titleKey, string icon, string route, string role, NavSection section = NavSection.Management, bool showOnMobile = false, int mobilePriority = 100, NavLinkMatch match = NavLinkMatch.Prefix)
        => new(titleKey, icon, route, new[] { role }, Array.Empty<string>(), match, section, showOnMobile, mobilePriority, RequireAuthenticated: true);
}

public enum NavSection
{
    Overview,
    Management,
    Access
}
