using Menu.UI.Auth;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Menu.UI.Navigation;

/// <summary>
/// Centralized navigation registry. Both the desktop sidebar and the mobile
/// bottom bar render from this list. To add or rename a page in the nav,
/// edit this file — never hard-code links in layout components.
/// </summary>
public static class AppNavigation
{
    public static IReadOnlyList<NavItem> Items { get; } = new[]
    {
        // ── Overview ─────────────────────────────────────────────────────────
        NavItem.Authenticated(
            titleKey: "Navigation.Dashboard",
            icon: Icons.Material.Filled.Dashboard,
            route: "/",
            section: NavSection.Overview,
            showOnMobile: true,
            mobilePriority: 1,
            match: NavLinkMatch.All),

        NavItem.ByPermission(
            titleKey: "Navigation.Sessions",
            icon: Icons.Material.Filled.History,
            route: "/sessions",
            permission: AppPermissions.SessionsView,
            section: NavSection.Overview),

        // ── Management ───────────────────────────────────────────────────────
        NavItem.ByPermission(
            titleKey: "Navigation.Restaurants",
            icon: Icons.Material.Filled.Store,
            route: "/restaurants",
            permission: AppPermissions.RestaurantsView,
            section: NavSection.Management),

        NavItem.ByPermission(
            titleKey: "Navigation.Menus",
            icon: Icons.Material.Filled.MenuBook,
            route: "/menus",
            permission: AppPermissions.MenusView,
            section: NavSection.Management,
            showOnMobile: true,
            mobilePriority: 2),

        NavItem.ByPermission(
            titleKey: "Navigation.Categories",
            icon: Icons.Material.Filled.Category,
            route: "/categories",
            permission: AppPermissions.CategoriesView,
            section: NavSection.Management),

        NavItem.ByPermission(
            titleKey: "Navigation.MenuItems",
            icon: Icons.Material.Filled.Fastfood,
            route: "/menu-items",
            permission: AppPermissions.MenuItemsView,
            section: NavSection.Management,
            showOnMobile: true,
            mobilePriority: 3),

        NavItem.ByPermission(
            titleKey: "Navigation.ItemOptions",
            icon: Icons.Material.Filled.Tune,
            route: "/item-options",
            permission: AppPermissions.ItemOptionsView,
            section: NavSection.Management),

        NavItem.ByPermission(
            titleKey: "Navigation.OptionValues",
            icon: Icons.Material.Filled.TaskAlt,
            route: "/option-values",
            permission: AppPermissions.OptionValuesView,
            section: NavSection.Management),

        // ── Access Control ───────────────────────────────────────────────────
        NavItem.ByPermission(
            titleKey: "Navigation.Users",
            icon: Icons.Material.Filled.People,
            route: "/users",
            permission: AppPermissions.UsersView,
            section: NavSection.Access,
            showOnMobile: true,
            mobilePriority: 4),

        NavItem.ByPermission(
            titleKey: "Navigation.Roles",
            icon: Icons.Material.Filled.AdminPanelSettings,
            route: "/roles",
            permission: AppPermissions.RolesView,
            section: NavSection.Access),

        NavItem.ByPermission(
            titleKey: "Navigation.Permissions",
            icon: Icons.Material.Filled.Key,
            route: "/permissions",
            permission: AppPermissions.PermissionsView,
            section: NavSection.Access),

        NavItem.ByRole(
            titleKey: "Navigation.AccessControl",
            icon: Icons.Material.Filled.Lock,
            route: "/access-control",
            role: AppRoles.SuperAdmin,
            section: NavSection.Access,
            showOnMobile: true,
            mobilePriority: 5),
    };
}
